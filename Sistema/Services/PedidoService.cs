// SqlException: identifica a violação de índice único na confirmação duplicada (QA B1)
using Microsoft.Data.SqlClient;
// Regex: normaliza os espaços do motivo da recusa (HF-006)
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Alterado: antes registrava compras e vendas dando baixa no estoque na hora.
    // Agora cuida só do ciclo do pedido do cliente:
    //   criar (Pendente, sem mexer no estoque) → confirmar (baixa o estoque e cria a Compra)
    //                                         → recusar (ADM) ou cancelar (o próprio cliente)
    // A venda direta do vendedor ficou no VendaService.
    public class PedidoService(ApplicationDbContext context)
    {
        // Cria o pedido pendente do cliente. O estoque não é baixado aqui, só na confirmação
        public async Task<(Pedido? Pedido, string? Erro)> CriarAsync(Usuario comprador, IReadOnlyList<(int ProdutoId, int Quantidade)> itens)
        {
            var (agrupados, erro) = ItensPedido.Preparar(itens);
            if (erro != null)
                return (null, erro);

            var ids = agrupados.Select(i => i.ProdutoId).ToList();
            var produtos = await context.Produtos
                .Where(p => ids.Contains(p.ProdutoId))
                .ToDictionaryAsync(p => p.ProdutoId);

            var pedido = new Pedido
            {
                DataHora = DateTime.Now,
                Status = StatusPedido.Pendente,
                CompradorId = comprador.UsuarioId,
                // Copia a localização do perfil: o painel por localidade usa a do momento do pedido
                Uf = comprador.Uf,
                Cidade = comprador.Cidade,
                CodigoMunicipio = comprador.CodigoMunicipio
            };

            foreach (var (produtoId, quantidade) in agrupados)
            {
                if (!produtos.TryGetValue(produtoId, out var produto))
                    return (null, "Produto não encontrado.");

                // Não aceita pedir mais do que existe hoje. Não é reserva: o estoque é conferido de novo na confirmação
                // Alterado (direção D, B2): mesma regra única de falta de estoque (FaltaEstoque)
                if (FaltaEstoque(produto.Estoque, quantidade))
                    return (null, $"Estoque insuficiente para {produto.Name}: restam {produto.Estoque} unidade(s).");

                // Guarda o preço atual: é o valor que o cliente viu e que vale na confirmação
                pedido.Itens.Add(new PedidoItem
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.Preco
                });
                pedido.Total += quantidade * produto.Preco;
            }

            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();
            return (pedido, null);
        }

        // ADM confirma: confere o estoque, dá baixa, cria a Compra e marca o pedido como Confirmado, tudo numa transação
        public async Task<(Compra? Compra, string? Erro)> ConfirmarAsync(int pedidoId, int admId)
        {
            // Transação: se algo falhar no meio, nem o estoque nem o pedido mudam
            await using var transacao = await context.Database.BeginTransactionAsync();

            // Corrigido (QA B1): dois ADMs confirmando ao mesmo tempo davam erro 500 (o índice único de Compra.PedidoId
            // barrava a segunda compra). Agora o pedido é "reservado" primeiro com um UPDATE condicional: o banco tranca a
            // linha, a segunda confirmação espera, e quando roda já encontra o pedido fora de Pendente (0 linhas afetadas).
            // Se esta confirmação falhar depois (falta de estoque), a transação é desfeita e o pedido volta a Pendente.
            var reservado = await context.Pedidos
                .Where(p => p.PedidoId == pedidoId && p.Status == StatusPedido.Pendente)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, StatusPedido.Confirmado));
            if (reservado == 0)
            {
                var statusAtual = await context.Pedidos
                    .Where(p => p.PedidoId == pedidoId)
                    .Select(p => (StatusPedido?)p.Status)
                    .FirstOrDefaultAsync();
                return (null, statusAtual == null
                    ? "Pedido não encontrado."
                    : $"Este pedido já foi {NomeStatus(statusAtual.Value).ToLowerInvariant()}.");
            }

            // Carregado depois da reserva: já vem com o status Confirmado e o RowVersion novo
            var pedido = await context.Pedidos
                .Include(p => p.Itens)
                .FirstAsync(p => p.PedidoId == pedidoId);

            var ids = pedido.Itens.Select(i => i.ProdutoId).ToList();
            var produtos = await context.Produtos
                .Where(p => ids.Contains(p.ProdutoId))
                .ToDictionaryAsync(p => p.ProdutoId);

            var compra = new Compra
            {
                DataHora = DateTime.Now,
                CompradorId = pedido.CompradorId,
                PedidoId = pedido.PedidoId,
                Total = pedido.Total,
                Uf = pedido.Uf,
                Cidade = pedido.Cidade,
                CodigoMunicipio = pedido.CodigoMunicipio
            };

            // Primeiro confere tudo, sem mexer no estoque; junta as faltas numa mensagem só para o ADM decidir se recusa
            var faltas = new List<string>();
            foreach (var item in pedido.Itens)
            {
                if (!produtos.TryGetValue(item.ProdutoId, out var produto))
                    faltas.Add($"produto {item.ProdutoId} não existe mais");
                // Alterado (direção D, B2): mesma regra única de falta de estoque (FaltaEstoque)
                else if (FaltaEstoque(produto.Estoque, item.Quantidade))
                    faltas.Add($"{produto.Name}: pedido {item.Quantidade}, em estoque {produto.Estoque}");
            }
            if (faltas.Count > 0)
                return (null, "Estoque insuficiente para confirmar: " + string.Join("; ", faltas) + ". Reponha o estoque ou recuse o pedido.");

            foreach (var item in pedido.Itens)
            {
                // A baixa no estoque acontece aqui, na confirmação
                produtos[item.ProdutoId].Estoque -= item.Quantidade;
                // Mantém o preço do pedido (o que o cliente aceitou), mesmo que o produto tenha mudado de preço
                compra.Itens.Add(new CompraItem
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                });
            }

            pedido.DataDecisao = compra.DataHora;
            pedido.DecididoPorId = admId;
            context.Compras.Add(compra);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Uma venda mexeu no estoque ao mesmo tempo (o pedido em si já está reservado acima)
                return (null, "O pedido ou o estoque mudou enquanto a confirmação era registrada. Atualize a tela e tente de novo.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // Corrigido (QA B1): defesa extra. Se o índice único de Compra.PedidoId ainda barrar uma compra duplicada,
                // a tela mostra a mensagem em vez de erro 500
                return (null, "Este pedido já foi confirmado por outra pessoa. Atualize a tela.");
            }
            await transacao.CommitAsync();
            return (compra, null);
        }

        // ADM recusa um pedido pendente. O estoque não muda (nunca foi baixado)
        public async Task<string?> RecusarAsync(int pedidoId, int admId, string? motivo)
        {
            var pedido = await context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
            if (pedido == null)
                return "Pedido não encontrado.";

            // Corrigido (HF-006): motivo em uma linha. Quebras (CRLF = 2 caracteres), tabs e espaços repetidos viram um espaço
            // antes de medir os 300, para o texto aceito pelo maxlength do navegador não ser recusado aqui
            motivo = string.IsNullOrWhiteSpace(motivo) ? null : Regex.Replace(motivo, @"\s+", " ").Trim();
            if (motivo?.Length > 300)
                return "O motivo pode ter no máximo 300 caracteres.";

            return await DecidirAsync(pedido, StatusPedido.Recusado, admId, motivo);
        }

        // O próprio cliente cancela o pedido enquanto ele ainda está pendente
        public async Task<string?> CancelarAsync(int pedidoId, int compradorId)
        {
            // Filtra pelo comprador: um cliente não consegue cancelar o pedido de outro trocando o id na URL
            var pedido = await context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == pedidoId && p.CompradorId == compradorId);
            if (pedido == null)
                return "Pedido não encontrado.";

            return await DecidirAsync(pedido, StatusPedido.Cancelado, compradorId, null);
        }

        // Novo (direção D, B2): regra única de falta de estoque. Usada ao criar, ao confirmar e no aviso da fila de Pedidos
        public static bool FaltaEstoque(int estoque, int quantidade) => estoque < quantidade;

        // Novo (direção D, B2): itens de pedidos PENDENTES que hoje não seriam confirmados por falta de estoque.
        // Chave = PedidoItemId; valor = estoque atual do produto. É só aviso: não reserva nada, e a ConfirmarAsync
        // confere de novo dentro da transação. Cada pedido é avaliado sozinho, como na confirmação
        public static Dictionary<int, int> FaltasDeEstoque(IEnumerable<Pedido> pedidos) =>
            pedidos.Where(p => p.Status == StatusPedido.Pendente)
                   .SelectMany(p => p.Itens)
                   .Where(i => i.Produto != null && FaltaEstoque(i.Produto.Estoque, i.Quantidade))
                   .ToDictionary(i => i.PedidoItemId, i => i.Produto!.Estoque);

        // Texto do status para mensagens e telas
        // (Corrigido, revisão D1/D2, achado 4: este comentário tinha ficado acima do bloco da B2)
        public static string NomeStatus(StatusPedido status) => status switch
        {
            StatusPedido.Pendente => "Pendente",
            StatusPedido.Confirmado => "Confirmado",
            StatusPedido.Recusado => "Recusado",
            StatusPedido.Cancelado => "Cancelado",
            _ => status.ToString()
        };

        // Fecha um pedido pendente como recusado ou cancelado, registrando data e autor
        private async Task<string?> DecidirAsync(Pedido pedido, StatusPedido novoStatus, int usuarioId, string? motivo)
        {
            if (pedido.Status != StatusPedido.Pendente)
                return $"Este pedido já foi {NomeStatus(pedido.Status).ToLowerInvariant()}.";

            pedido.Status = novoStatus;
            pedido.DataDecisao = DateTime.Now;
            pedido.DecididoPorId = usuarioId;
            pedido.Motivo = motivo;
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return "O pedido mudou enquanto a operação era registrada. Atualize a tela e tente de novo.";
            }
            return null;
        }
    }
}
