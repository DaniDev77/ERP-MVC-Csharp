using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Helpers;
using Sistema.Models;
using Sistema.Services;

// Páginas de desempenho: ADM vê compras e vendas de todos; o vendedor vê só as próprias vendas.
// Alterado: os números vêm das tabelas Compra (só compras confirmadas) e Venda, e não mais da tabela Pedido
public class DesempenhoController(ApplicationDbContext context, UsuarioLogadoService usuarioLogado) : Controller
{
    // GET: DESEMPENHO?periodo=mes
    // Compras e vendas de todos os usuários no período escolhido
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Index(string periodo = "mes")
    {
        // Alterado: o cálculo de período foi para Helpers/Periodos (mesmas regras).
        // Corrigido (QA B6): valor desconhecido (inclusive "todos", que só vale na lista de pedidos) volta a ser "mes"
        periodo = Periodos.Normalizar(periodo);
        var (inicio, fim) = Periodos.Calcular(periodo);
        var compras = context.Compras.Where(c => c.DataHora >= inicio && c.DataHora < fim);
        var vendas = context.Vendas.Where(v => v.DataHora >= inicio && v.DataHora < fim);

        var modelo = new DesempenhoViewModel
        {
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            QtdCompras = await compras.CountAsync(),
            ValorCompras = await compras.SumAsync(c => c.Total),
            QtdVendas = await vendas.CountAsync(),
            ValorVendas = await vendas.SumAsync(v => v.Total)
        };

        // Quanto cada vendedor vendeu no período, do maior para o menor.
        // Alterado: agrupa pelo id do vendedor (antes era pelo nome, e dois vendedores com o mesmo nome se somavam)
        var porVendedor = await vendas
            .GroupBy(v => new { v.VendedorId, v.Vendedor!.Name })
            .Select(g => new { g.Key.Name, Quantidade = g.Count(), Valor = g.Sum(v => v.Total) })
            .ToListAsync();
        modelo.PorVendedor = porVendedor
            .Select(l => new LinhaVendedor { Nome = l.Name, Quantidade = l.Quantidade, Valor = l.Valor })
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

        // Corrigido (QA B6): mesmo tratamento de período desconhecido do Index
        periodo = Periodos.Normalizar(periodo);
        var (inicio, fim) = Periodos.Calcular(periodo);
        // Alterado: lê da tabela Venda
        var vendas = context.Vendas.Where(v => v.VendedorId == vendedor.UsuarioId
                                               && v.DataHora >= inicio && v.DataHora < fim);

        var modelo = new DesempenhoViewModel
        {
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            QtdVendas = await vendas.CountAsync(),
            ValorVendas = await vendas.SumAsync(v => v.Total)
        };

        ViewBag.Acao = nameof(Vendedor);
        ViewBag.MostrarCompras = false;
        return View(modelo);
    }

    // Novo — GET: DESEMPENHO/Localidades?periodo=mes&tipo=compras&regiao=sudeste&uf=SP&ver=cidade
    // Índice de compras por região, estado e cidade.
    // Alterado (HF-007): usa só a localização gravada na operação. Uma compra fica na cidade em que foi feita;
    // sem localização gravada (não informada na época ou apagada pelo cliente, B2) conta como "Sem localização"
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Localidades(string periodo = "mes", string tipo = "compras", string? regiao = null, string? uf = null, string? ver = null)
    {
        periodo = Periodos.Normalizar(periodo);
        tipo = tipo is "vendas" or "todos" ? tipo : "compras";
        uf = MunicipioService.UfValida(uf?.ToUpperInvariant()) ? uf!.ToUpperInvariant() : null;
        // Novo (HF-007): região inválida vira Brasil todo
        regiao = regiao?.Trim().ToLowerInvariant();
        regiao = MunicipioService.RegiaoValida(regiao) ? regiao : null;
        // Região e UF de regiões diferentes: vale a UF. Uma UF sozinha (?uf=SP, links do HF-003) não estreita o PorUf
        if (uf != null && regiao != null) regiao = MunicipioService.RegiaoDaUf(uf);
        // Novo (HF-007): qual gráfico fica em destaque (link sem JS); valor desconhecido vira "uf"
        ver = ver?.ToLowerInvariant() switch { "regiao" => "regiao", "cidade" => "cidade", _ => "uf" };
        var (inicio, fim) = Periodos.Calcular(periodo);

        // Cada operação vira (UF, código, cidade, total). O agrupamento é feito no banco
        var linhas = new List<Agrupado>();
        if (tipo is "compras" or "todos")
        {
            linhas.AddRange(await context.Compras
                .Where(c => c.DataHora >= inicio && c.DataHora < fim)
                // Alterado (HF-007): sem fallback para a localização atual do comprador (que reatribuía compras antigas à cidade nova)
                .Select(c => new { c.Uf, Codigo = c.CodigoMunicipio, c.Cidade, c.Total })
                .GroupBy(x => new { x.Uf, x.Codigo, x.Cidade })
                .Select(g => new Agrupado(g.Key.Uf, g.Key.Codigo, g.Key.Cidade, g.Count(), g.Sum(x => x.Total)))
                .ToListAsync());
        }
        if (tipo is "vendas" or "todos")
        {
            linhas.AddRange(await context.Vendas
                .Where(v => v.DataHora >= inicio && v.DataHora < fim)
                // Alterado (HF-007): sem fallback para a localização atual do comprador
                .Select(v => new { v.Uf, Codigo = v.CodigoMunicipio, v.Cidade, v.Total })
                .GroupBy(x => new { x.Uf, x.Codigo, x.Cidade })
                .Select(g => new Agrupado(g.Key.Uf, g.Key.Codigo, g.Key.Cidade, g.Count(), g.Sum(x => x.Total)))
                .ToListAsync());
        }

        // Alterado (HF-007): UF fora da lista do IBGE também conta como sem localização (assim RegiaoDaUf nunca dá nulo)
        var comLocal = linhas.Where(l => MunicipioService.UfValida(l.Uf)).ToList();
        var semLocal = linhas.Where(l => !MunicipioService.UfValida(l.Uf)).ToList();
        var valorComLocal = comLocal.Sum(l => l.Valor);
        // Percentual sempre sobre o Brasil com localização, nas três listas
        decimal Fatia(decimal valor) => valorComLocal == 0 ? 0 : Math.Round(valor / valorComLocal * 100, 1);
        // Novo (HF-007): recorte da região escolhida (sem região = Brasil todo)
        bool NaRegiao(string? u) => regiao == null || MunicipioService.RegiaoDaUf(u) == regiao;

        var modelo = new LocalidadesViewModel
        {
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            Tipo = tipo,
            Regiao = regiao,
            Uf = uf,
            Ver = ver,
            QtdTotal = linhas.Sum(l => l.Quantidade),
            ValorTotal = linhas.Sum(l => l.Valor),
            QtdSemLocal = semLocal.Sum(l => l.Quantidade),
            ValorSemLocal = semLocal.Sum(l => l.Valor),
            // Novo (HF-007): ranking por região, sempre do Brasil todo
            PorRegiao = comLocal
                .GroupBy(l => MunicipioService.RegiaoDaUf(l.Uf)!)
                .Select(g => new LinhaLocalidade
                {
                    Chave = g.Key, Regiao = g.Key, Nome = MunicipioService.NomeRegiao(g.Key),
                    Quantidade = g.Sum(l => l.Quantidade), Valor = g.Sum(l => l.Valor), Percentual = Fatia(g.Sum(l => l.Valor))
                })
                .OrderByDescending(l => l.Valor)
                .ToList(),
            // Alterado (HF-007): só a região escolhida, quando houver
            PorUf = comLocal
                .Where(l => NaRegiao(l.Uf))
                .GroupBy(l => l.Uf!)
                .Select(g => new LinhaLocalidade
                {
                    Chave = g.Key, Uf = g.Key, Regiao = MunicipioService.RegiaoDaUf(g.Key)!, Nome = MunicipioService.NomeUf(g.Key),
                    Quantidade = g.Sum(l => l.Quantidade), Valor = g.Sum(l => l.Valor), Percentual = Fatia(g.Sum(l => l.Valor))
                })
                .OrderByDescending(l => l.Valor)
                .ToList(),
            // Ranking de cidades: da UF escolhida; sem UF, da região; sem as duas, do Brasil todo.
            // Agrupa pelo código IBGE (cidades homônimas não se misturam)
            PorCidade = comLocal
                .Where(l => uf != null ? l.Uf == uf : NaRegiao(l.Uf))
                .GroupBy(l => new { l.Uf, l.Codigo })
                .Select(g => new LinhaLocalidade
                {
                    Chave = g.Key.Codigo?.ToString() ?? "", Uf = g.Key.Uf!, Regiao = MunicipioService.RegiaoDaUf(g.Key.Uf)!,
                    Nome = g.First().Cidade ?? "Cidade não informada",
                    Quantidade = g.Sum(l => l.Quantidade), Valor = g.Sum(l => l.Valor), Percentual = Fatia(g.Sum(l => l.Valor))
                })
                .OrderByDescending(l => l.Valor)
                .Take(20)
                .ToList()
        };

        return View(modelo);
    }

    // Resultado do GROUP BY de compras/vendas por localidade
    private sealed record Agrupado(string? Uf, int? Codigo, string? Cidade, int Quantidade, decimal Valor);
}
