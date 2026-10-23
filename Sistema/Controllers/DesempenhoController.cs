using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

// Páginas de desempenho: ADM vê compras e vendas de todos; o vendedor vê só as próprias vendas
public class DesempenhoController(ApplicationDbContext context, UsuarioLogadoService usuarioLogado) : Controller
{
    // GET: DESEMPENHO?periodo=mes
    // Compras e vendas de todos os usuários no período escolhido
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Index(string periodo = "mes")
    {
        var (inicio, fim) = CalcularPeriodo(periodo);
        var pedidos = context.Pedidos.Where(p => p.DataHora >= inicio && p.DataHora < fim);

        var modelo = new DesempenhoViewModel
        {
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            QtdCompras = await pedidos.CountAsync(p => p.Tipo == TipoPedido.Compra),
            ValorCompras = await pedidos.Where(p => p.Tipo == TipoPedido.Compra).SumAsync(p => p.Total),
            QtdVendas = await pedidos.CountAsync(p => p.Tipo == TipoPedido.Venda),
            ValorVendas = await pedidos.Where(p => p.Tipo == TipoPedido.Venda).SumAsync(p => p.Total)
        };

        // Quanto cada vendedor vendeu no período, do maior para o menor. Agrupa em memória,
        // porque o nome do vendedor vem da tabela Usuario
        var vendas = await pedidos
            .Where(p => p.Tipo == TipoPedido.Venda && p.VendedorId != null)
            .Select(p => new { p.VendedorId, Nome = p.Vendedor!.Name, p.Total })
            .ToListAsync();
        modelo.PorVendedor = vendas
            .GroupBy(v => v.Nome)
            .Select(g => new LinhaVendedor { Nome = g.Key, Quantidade = g.Count(), Valor = g.Sum(v => v.Total) })
            .OrderByDescending(l => l.Valor)
            .ToList();

        ViewBag.Acao = nameof(Index);
        ViewBag.MostrarCompras = true;
        return View(modelo);
    }

    // GET: DESEMPENHO/Vendedor?periodo=mes
    // Só as vendas feitas pelo próprio vendedor logado
    [Authorize(Policy = Politicas.SomenteVendedor)]
    public async Task<IActionResult> Vendedor(string periodo = "mes")
    {
        var vendedor = await usuarioLogado.ObterAsync(User);
        if (vendedor == null)
            return Forbid();

        var (inicio, fim) = CalcularPeriodo(periodo);
        var vendas = context.Pedidos.Where(p => p.Tipo == TipoPedido.Venda
                                                && p.VendedorId == vendedor.UsuarioId
                                                && p.DataHora >= inicio && p.DataHora < fim);

        var modelo = new DesempenhoViewModel
        {
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            QtdVendas = await vendas.CountAsync(),
            ValorVendas = await vendas.SumAsync(p => p.Total)
        };

        ViewBag.Acao = nameof(Vendedor);
        ViewBag.MostrarCompras = false;
        return View(modelo);
    }

    // Converte o nome do período em data inicial; o fim é amanhã (exclusivo), ou seja, inclui hoje
    private static (DateTime Inicio, DateTime Fim) CalcularPeriodo(string periodo)
    {
        var hoje = DateTime.Today;

        var inicio = periodo switch
        {
            // Semana começa na segunda-feira
            "semana" => hoje.AddDays(-(((int)hoje.DayOfWeek + 6) % 7)),
            // Bimestres: jan-fev, mar-abr, mai-jun, jul-ago, set-out, nov-dez
            "bimestre" => new DateTime(hoje.Year, ((hoje.Month - 1) / 2) * 2 + 1, 1),
            // Semestres: jan-jun e jul-dez
            "semestre" => new DateTime(hoje.Year, hoje.Month <= 6 ? 1 : 7, 1),
            "ano" => new DateTime(hoje.Year, 1, 1),
            _ => new DateTime(hoje.Year, hoje.Month, 1)
        };

        return (inicio, hoje.AddDays(1));
    }
}
