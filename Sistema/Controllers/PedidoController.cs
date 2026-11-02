using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Helpers;
using Sistema.Models;
using Sistema.Services;

// Novo: pedidos dos clientes.
//  - ADM: lista, filtra por status e período, confirma (vira Compra e baixa o estoque) ou recusa.
//  - Cliente (Padrão/ADM): acompanha os próprios pedidos e cancela os que ainda estão pendentes.
[Authorize]
public class PedidoController(ApplicationDbContext context, PedidoService pedidos, UsuarioLogadoService usuarioLogado) : Controller
{
    // Valores aceitos no filtro de status (query string → enum; "todos" = sem filtro)
    private static readonly Dictionary<string, StatusPedido> FiltrosStatus = new()
    {
        ["pendente"] = StatusPedido.Pendente,
        ["confirmado"] = StatusPedido.Confirmado,
        ["recusado"] = StatusPedido.Recusado,
        ["cancelado"] = StatusPedido.Cancelado
    };

    // GET: PEDIDO?status=pendente&periodo=todos
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Index(string status = "pendente", string periodo = "todos")
    {
        status = status == "todos" || FiltrosStatus.ContainsKey(status) ? status : "pendente";
        periodo = Periodos.Normalizar(periodo, aceitaTodos: true);
        var (inicio, fim) = Periodos.Calcular(periodo);

        var doPeriodo = context.Pedidos.Where(p => p.DataHora >= inicio && p.DataHora < fim);

        var modelo = new PedidosAdminViewModel
        {
            Status = status,
            Periodo = periodo,
            Inicio = inicio,
            Fim = fim,
            // Contagem por status em uma consulta só (GROUP BY no banco)
            Contagem = await doPeriodo
                .GroupBy(p => p.Status)
                .Select(g => new { g.Key, Qtd = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Qtd)
        };

        var filtrados = FiltrosStatus.TryGetValue(status, out var s) ? doPeriodo.Where(p => p.Status == s) : doPeriodo;
        // Pendentes: o mais antigo primeiro (fila de atendimento). Demais: o mais recente primeiro
        filtrados = status == "pendente"
            ? filtrados.OrderBy(p => p.DataHora)
            : filtrados.OrderByDescending(p => p.DataHora);

        // Limite de 200 linhas para a tela não pesar; os filtros servem para achar os mais antigos.
        // AsSplitQuery: evita multiplicar as linhas do pedido pelos itens no SQL
        modelo.Pedidos = await filtrados
            .Include(p => p.Comprador)
            .Include(p => p.DecididoPor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .AsSplitQuery()
            .Take(200)
            .ToListAsync();

        return View(modelo);
    }

    // POST: PEDIDO/Confirmar/5 — confere e baixa o estoque e cria a Compra
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Confirmar(int id, string? status, string? periodo)
    {
        var adm = await usuarioLogado.ObterAsync(User);
        if (adm == null)
            return Forbid();

        var (compra, erro) = await pedidos.ConfirmarAsync(id, adm.UsuarioId);
        if (erro == null)
            TempData["Sucesso"] = $"Pedido nº {id} confirmado. Compra nº {compra!.CompraId} registrada e estoque atualizado.";
        else
            TempData["Erro"] = erro;

        // Volta para a mesma lista e com os mesmos filtros
        return RedirectToAction(nameof(Index), new { status, periodo });
    }

    // POST: PEDIDO/Recusar/5 — o estoque não muda (nunca foi baixado)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Recusar(int id, string? motivo, string? status, string? periodo)
    {
        var adm = await usuarioLogado.ObterAsync(User);
        if (adm == null)
            return Forbid();

        var erro = await pedidos.RecusarAsync(id, adm.UsuarioId, motivo);
        if (erro == null)
            TempData["Sucesso"] = $"Pedido nº {id} recusado.";
        else
            TempData["Erro"] = erro;

        return RedirectToAction(nameof(Index), new { status, periodo });
    }

    // GET: PEDIDO/Meus — os pedidos do cliente logado, do mais recente para o mais antigo
    [Authorize(Policy = Politicas.Padrao)]
    public async Task<IActionResult> Meus()
    {
        var cliente = await usuarioLogado.ObterAsync(User);
        if (cliente == null)
            return Forbid();

        var meus = await context.Pedidos
            .Where(p => p.CompradorId == cliente.UsuarioId)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .OrderByDescending(p => p.DataHora)
            .AsSplitQuery()
            .Take(100)
            .ToListAsync();

        return View(meus);
    }

    // POST: PEDIDO/Cancelar/5 — só o dono do pedido, e só enquanto pendente
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Padrao)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cliente = await usuarioLogado.ObterAsync(User);
        if (cliente == null)
            return Forbid();

        var erro = await pedidos.CancelarAsync(id, cliente.UsuarioId);
        if (erro == null)
            TempData["Sucesso"] = $"Pedido nº {id} cancelado.";
        else
            TempData["Erro"] = erro;

        return RedirectToAction(nameof(Meus));
    }
}
