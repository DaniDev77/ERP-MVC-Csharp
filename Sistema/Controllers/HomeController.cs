using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

namespace Sistema.Controllers
{
    public class HomeController(ApplicationDbContext context, UsuarioLogadoService usuarioLogado, IAuthorizationService authorization) : Controller
    {
        // Painel inicial: números do mês conforme a função de quem está logado
        public async Task<IActionResult> Index()
        {
            var modelo = new InicioViewModel { Logado = User.Identity?.IsAuthenticated == true };
            if (!modelo.Logado)
                return View(modelo);

            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1);
            modelo.Mes = inicioMes;

            modelo.EhAdmin = (await authorization.AuthorizeAsync(User, Politicas.Admin)).Succeeded;
            var usuario = await usuarioLogado.ObterAsync(User);
            modelo.EhVendedor = usuario?.FuncaoId == Politicas.FuncaoVendedorId;
            modelo.EhPadrao = usuario?.FuncaoId == Politicas.FuncaoPadraoId;
            // Conta sem cadastro ou sem função: ainda não pode vender nem comprar
            modelo.Pendente = !modelo.EhAdmin && !modelo.EhVendedor && !modelo.EhPadrao;
            // Vazio quando a conta ainda não tem cadastro: a tela oferece completá-lo
            modelo.NomeUsuario = usuario?.Name ?? "";

            if (modelo.EhAdmin)
            {
                // Pedidos do mês, trazidos só com o que os cálculos precisam
                var pedidosMes = await context.Pedidos
                    .Where(p => p.DataHora >= inicioMes && p.DataHora < fimMes)
                    .Select(p => new { p.DataHora, p.Tipo, p.Total })
                    .ToListAsync();

                var vendas = pedidosMes.Where(p => p.Tipo == TipoPedido.Venda).ToList();
                var compras = pedidosMes.Where(p => p.Tipo == TipoPedido.Compra).ToList();
                modelo.ValorVendasMes = vendas.Sum(p => p.Total);
                modelo.QtdVendasMes = vendas.Count;
                modelo.ValorComprasMes = compras.Sum(p => p.Total);
                modelo.QtdComprasMes = compras.Count;

                // Vendas somadas por dia, do dia 1 até hoje
                var valoresPorDia = new decimal[hoje.Day];
                foreach (var venda in vendas)
                    valoresPorDia[venda.DataHora.Day - 1] += venda.Total;
                modelo.PontosVendasPorDia = MontarPontos(valoresPorDia);
                modelo.MaiorVendaDoDia = valoresPorDia.Max();
                // Os mesmos totais diários, para o gráfico de colunas da tela inicial
                modelo.VendasPorDia = valoresPorDia;

                // Estoque: contagem e valor total, mais os produtos que precisam de reposição
                modelo.TotalEstoqueBaixo = await context.Produtos.CountAsync(p => p.Estoque <= Produto.EstoqueMinimo);
                modelo.ValorEmEstoque = await context.Produtos.SumAsync(p => p.Preco * p.Estoque);
                // Contagens do resumo de estoque: produtos cadastrados e produtos zerados
                modelo.TotalProdutos = await context.Produtos.CountAsync();
                modelo.TotalSemEstoque = await context.Produtos.CountAsync(p => p.Estoque == 0);
                // Os cinco menores estoques da loja (abaixo do mínimo ou não): a lista de reposição nunca fica vazia
                // enquanto houver produto, e os que estão no mínimo ou abaixo aparecem marcados
                modelo.EstoqueBaixo = await context.Produtos
                    .OrderBy(p => p.Estoque)
                    .Take(5)
                    .ToListAsync();

                modelo.UltimosPedidos = await context.Pedidos
                    .Include(p => p.Comprador)
                    .Include(p => p.Vendedor)
                    .OrderByDescending(p => p.DataHora)
                    .Take(6)
                    .ToListAsync();
            }
            else if (usuario != null)
            {
                // Vendedor: as próprias vendas do mês. Padrão: as próprias compras do mês
                var meus = context.Pedidos.Where(p => p.DataHora >= inicioMes && p.DataHora < fimMes);
                if (modelo.EhVendedor)
                {
                    var vendas = meus.Where(p => p.Tipo == TipoPedido.Venda && p.VendedorId == usuario.UsuarioId);
                    modelo.QtdVendasMes = await vendas.CountAsync();
                    modelo.ValorVendasMes = await vendas.SumAsync(p => p.Total);
                }
                else if (modelo.EhPadrao)
                {
                    var compras = meus.Where(p => p.Tipo == TipoPedido.Compra && p.CompradorId == usuario.UsuarioId);
                    modelo.QtdComprasMes = await compras.CountAsync();
                    modelo.ValorComprasMes = await compras.SumAsync(p => p.Total);
                }
            }

            return View(modelo);
        }

        // Converte os totais diários em pontos de um SVG de 720x180 (y invertido: valor maior fica mais alto)
        private static string MontarPontos(decimal[] valores)
        {
            const double largura = 720, altura = 180, margem = 6;
            var maior = valores.Max();
            var escala = maior > 0 ? maior : 1;
            var n = valores.Length;

            var pontos = new List<string>();
            for (var i = 0; i < n; i++)
            {
                // Com um único dia, o ponto fica no meio do gráfico
                var x = n == 1 ? largura / 2 : i * largura / (n - 1);
                var y = altura - margem - (double)(valores[i] / escala) * (altura - 2 * margem);
                pontos.Add(x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture));
            }
            return string.Join(" ", pontos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
