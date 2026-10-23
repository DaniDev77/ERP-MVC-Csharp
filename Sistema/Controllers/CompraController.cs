using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

// Compras: usuário padrão (e ADM) compra produtos para si mesmo
[Authorize(Policy = Politicas.Padrao)]
public class CompraController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PedidoService _pedidos;
    private readonly UsuarioLogadoService _usuarioLogado;

    public CompraController(ApplicationDbContext context, PedidoService pedidos, UsuarioLogadoService usuarioLogado)
    {
        _context = context;
        _pedidos = pedidos;
        _usuarioLogado = usuarioLogado;
    }

    // GET: COMPRA/Create
    public async Task<IActionResult> Create()
    {
        await CarregarProdutos();
        return View(new NovoPedidoViewModel { Linhas = Enumerable.Range(0, 8).Select(_ => new LinhaPedido()).ToList() });
    }

    // POST: COMPRA/Create
    // O comprador é sempre o usuário logado; a compra não tem vendedor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovoPedidoViewModel model)
    {
        var comprador = await _usuarioLogado.ObterAsync(User);
        if (comprador == null)
            return Forbid();

        // Ignora as linhas em branco do formulário
        var itens = model.Linhas
            .Where(l => l.ProdutoId > 0)
            .Select(l => (l.ProdutoId, l.Quantidade))
            .ToList();

        var (pedido, erro) = await _pedidos.RegistrarAsync(TipoPedido.Compra, comprador.UsuarioId, null, itens);
        if (erro == null)
        {
            TempData["Sucesso"] = $"Compra registrada: {pedido!.Itens.Count} produto(s), total {pedido.Total:C2}.";
            return RedirectToAction(nameof(Create));
        }
        ModelState.AddModelError(string.Empty, erro);

        await CarregarProdutos();
        return View(model);
    }

    // Lista os produtos com estoque para o usuário escolher
    private async Task CarregarProdutos()
    {
        var produtos = await _context.Produtos
            .Where(p => p.Estoque > 0)
            .OrderBy(p => p.Name)
            .Select(p => new { p.ProdutoId, Texto = p.Name + " - " + p.Preco.ToString("C2") + " (estoque: " + p.Estoque + ")" })
            .ToListAsync();
        ViewBag.Produtos = new SelectList(produtos, "ProdutoId", "Texto");
        ViewBag.ExigeComprador = false;
    }
}
