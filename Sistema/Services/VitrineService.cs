using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Novo (lote B10, HF-015): o catálogo público da vitrine, já projetado e já em cache. Serve a home do
    // visitante ("/"), que é anônima: nada aqui depende de quem está pedindo, e por isso o cache é único
    // para todos. A fronteira de segurança desta tela não é autorização, é a projeção: o que não está em
    // ProdutoVitrine não sai do servidor (parecer-home-publica.md, RV5 e §8.4).
    public sealed class VitrineService(ApplicationDbContext context, IMemoryCache cache)
    {
        // Teto de linhas servidas sem paginação. O número real de produtos à venda vai em Total, e a
        // página avisa quando a lista está cortada (RV9: sem ?pagina=N numa página anônima)
        private const int Teto = 60;

        // Janela do cache. Consequência aceita: uma alteração de preço ou de estoque aparece na página
        // pública em até 60 segundos. Se incomodar, a correção certa não é diminuir o tempo, é invalidar
        // o cache depois do SaveChanges do ProdutoController — lote próprio, fora do B10 (RV8)
        private const int CacheSegundos = 60;

        private const string Chave = "vitrine:catalogo";

        public async Task<VitrineCatalogo> ObterAsync()
        {
            if (cache.TryGetValue(Chave, out VitrineCatalogo? guardado) && guardado != null)
                return guardado;

            // Decisão do dono (2026-11-13): só produto COM ESTOQUE aparece na home pública. O recorte é
            // filtro no servidor — o número do estoque nunca sai do banco, nem aparece na tela. É a mesma
            // regra que o PDV usa para permitir a venda (CompraController/VendaController: Estoque > 0)
            var itens = await context.Produtos
                .Where(p => p.Estoque > 0)
                .OrderBy(p => p.Name)
                .Take(Teto)
                .Select(p => new ProdutoVitrine(
                    p.Name,
                    p.TipoProduto == null ? null : p.TipoProduto.TipoNome,
                    p.Preco,
                    true))
                .ToListAsync();

            // A contagem segue o mesmo recorte da lista: a página não pode dizer "12 produtos" e mostrar 3
            // (parecer-home-publica.md §9). Com o filtro, o total do catálogo não é publicado
            var total = await context.Produtos.CountAsync(p => p.Estoque > 0);

            var catalogo = new VitrineCatalogo(itens, total);
            cache.Set(Chave, catalogo, TimeSpan.FromSeconds(CacheSegundos));
            return catalogo;
        }
    }
}
