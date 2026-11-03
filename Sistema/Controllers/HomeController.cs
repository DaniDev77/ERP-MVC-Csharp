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
                // Alterado: vendas e compras do mês vêm das tabelas Venda e Compra (só compras confirmadas contam),
                // trazidas só com o que os cálculos precisam
                var vendas = await context.Vendas
                    .Where(v => v.DataHora >= inicioMes && v.DataHora < fimMes)
                    .Select(v => new { v.DataHora, v.Total })
                    .ToListAsync();
                var compras = await context.Compras
                    .Where(c => c.DataHora >= inicioMes && c.DataHora < fimMes)
                    .Select(c => new { c.DataHora, c.Total })
                    .ToListAsync();
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

                // Alterado: "Últimos pedidos" junta as 6 vendas e as 6 compras mais recentes e fica com as 6 mais novas
                var ultimasVendas = await context.Vendas
                    .OrderByDescending(v => v.DataHora)
                    .Take(6)
                    .Select(v => new Movimentacao { EhVenda = true, DataHora = v.DataHora, Cliente = v.Comprador!.Name, Vendedor = v.Vendedor!.Name, Total = v.Total })
                    .ToListAsync();
                var ultimasCompras = await context.Compras
                    .OrderByDescending(c => c.DataHora)
                    .Take(6)
                    .Select(c => new Movimentacao { EhVenda = false, DataHora = c.DataHora, Cliente = c.Comprador!.Name, Total = c.Total })
                    .ToListAsync();
                modelo.UltimosPedidos = ultimasVendas.Concat(ultimasCompras)
                    .OrderByDescending(m => m.DataHora)
                    .Take(6)
                    .ToList();

                // Novo: pedidos de clientes esperando a confirmação do ADM
                modelo.PedidosPendentes = await context.Pedidos.CountAsync(p => p.Status == StatusPedido.Pendente);

                // Novo (direção D, B7): os 5 pendentes mais antigos (índice Status + DataHora), só as colunas que a fila mostra
                modelo.FilaPendentes = await context.Pedidos
                    .Where(p => p.Status == StatusPedido.Pendente)
                    .OrderBy(p => p.DataHora)
                    .Take(5)
                    .Select(p => new PedidoNaFila
                    {
                        PedidoId = p.PedidoId,
                        DataHora = p.DataHora,
                        Cliente = p.Comprador!.Name,
                        Unidades = p.Itens.Sum(i => i.Quantidade),
                        Total = p.Total,
                        Cidade = p.Cidade,
                        Uf = p.Uf
                    })
                    .ToListAsync();
            }
            else if (usuario != null)
            {
                // Vendedor: as próprias vendas do mês. Padrão: as próprias compras confirmadas do mês
                // Alterado: lê das tabelas Venda e Compra
                if (modelo.EhVendedor)
                {
                    var vendas = context.Vendas.Where(v => v.DataHora >= inicioMes && v.DataHora < fimMes && v.VendedorId == usuario.UsuarioId);
                    modelo.QtdVendasMes = await vendas.CountAsync();
                    modelo.ValorVendasMes = await vendas.SumAsync(v => v.Total);
                }
                else if (modelo.EhPadrao)
                {
                    var compras = context.Compras.Where(c => c.DataHora >= inicioMes && c.DataHora < fimMes && c.CompradorId == usuario.UsuarioId);
                    modelo.QtdComprasMes = await compras.CountAsync();
                    modelo.ValorComprasMes = await compras.SumAsync(c => c.Total);
                    // Novo: os próprios pedidos ainda aguardando a loja
                    modelo.PedidosPendentes = await context.Pedidos.CountAsync(p => p.CompradorId == usuario.UsuarioId && p.Status == StatusPedido.Pendente);
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
