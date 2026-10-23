using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Registra compras e vendas: confere o estoque, dá baixa nas quantidades e salva tudo em uma transação
    public class PedidoService(ApplicationDbContext context)
    {
        public async Task<(Pedido? Pedido, string? Erro)> RegistrarAsync(TipoPedido tipo, int compradorId, int? vendedorId, IReadOnlyList<(int ProdutoId, int Quantidade)> itens)
        {
            // Pedido sem produto não faz sentido
            if (itens.Count == 0)
                return (null, "Selecione pelo menos um produto.");

            // Quantidade zero ou negativa não pode dar baixa no estoque
            if (itens.Any(i => i.Quantidade <= 0))
                return (null, "A quantidade deve ser maior que zero.");

            // Se o mesmo produto aparecer em duas linhas, as quantidades são somadas
            var agrupados = itens
                .GroupBy(i => i.ProdutoId)
                .Select(g => (ProdutoId: g.Key, Quantidade: g.Sum(x => x.Quantidade)))
                .ToList();

            // Transação: se algo falhar no meio, nenhum estoque é alterado
            await using var transacao = await context.Database.BeginTransactionAsync();

            var ids = agrupados.Select(i => i.ProdutoId).ToList();
            var produtos = await context.Produtos
                .Where(p => ids.Contains(p.ProdutoId))
                .ToDictionaryAsync(p => p.ProdutoId);

            var pedido = new Pedido
            {
                Tipo = tipo,
                DataHora = DateTime.Now,
                CompradorId = compradorId,
                VendedorId = vendedorId
            };

            foreach (var (produtoId, quantidade) in agrupados)
            {
                if (!produtos.TryGetValue(produtoId, out var produto))
                    return (null, "Produto não encontrado.");

                // Não deixa vender mais do que existe no estoque
                if (produto.Estoque < quantidade)
                    return (null, $"Estoque insuficiente para {produto.Name}: restam {produto.Estoque} unidade(s).");

                // Baixa no estoque a cada venda ou compra
                produto.Estoque -= quantidade;

                // Guarda o preço atual, para que mudanças futuras no preço não alterem o histórico
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
            await transacao.CommitAsync();

            return (pedido, null);
        }
    }
}
