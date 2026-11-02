using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Novo: venda registrada direto pelo vendedor ou ADM (sem pedido). É a lógica que antes ficava no
    // PedidoService.RegistrarAsync com TipoPedido.Venda: confere o estoque, dá baixa e salva numa transação
    public class VendaService(ApplicationDbContext context)
    {
        public async Task<(Venda? Venda, string? Erro)> RegistrarAsync(Usuario comprador, int vendedorId, IReadOnlyList<(int ProdutoId, int Quantidade)> itens)
        {
            var (agrupados, erro) = ItensPedido.Preparar(itens);
            if (erro != null)
                return (null, erro);

            // Transação: se algo falhar no meio, nenhum estoque é alterado
            await using var transacao = await context.Database.BeginTransactionAsync();

            var ids = agrupados.Select(i => i.ProdutoId).ToList();
            var produtos = await context.Produtos
                .Where(p => ids.Contains(p.ProdutoId))
                .ToDictionaryAsync(p => p.ProdutoId);

            var venda = new Venda
            {
                DataHora = DateTime.Now,
                CompradorId = comprador.UsuarioId,
                VendedorId = vendedorId,
                // Localização do comprador no momento da venda, para o painel por localidade
                Uf = comprador.Uf,
                Cidade = comprador.Cidade,
                CodigoMunicipio = comprador.CodigoMunicipio
            };

            foreach (var (produtoId, quantidade) in agrupados)
            {
                if (!produtos.TryGetValue(produtoId, out var produto))
                    return (null, "Produto não encontrado.");

                // Não deixa vender mais do que existe no estoque
                if (produto.Estoque < quantidade)
                    return (null, $"Estoque insuficiente para {produto.Name}: restam {produto.Estoque} unidade(s).");

                // Baixa no estoque a cada venda
                produto.Estoque -= quantidade;

                // Guarda o preço atual, para que mudanças futuras no preço não alterem o histórico
                venda.Itens.Add(new VendaItem
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.Preco
                });
                venda.Total += quantidade * produto.Preco;
            }

            context.Vendas.Add(venda);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Outra operação mexeu no estoque ao mesmo tempo: nada é gravado (a transação é desfeita ao sair)
                return (null, "O estoque mudou enquanto a operação era registrada. Confira as quantidades e tente de novo.");
            }
            await transacao.CommitAsync();

            return (venda, null);
        }
    }
}
