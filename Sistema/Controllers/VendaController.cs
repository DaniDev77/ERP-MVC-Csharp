using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

// Registro de vendas: vendedor (e ADM) vende para um usuário padrão
[Authorize(Policy = Politicas.Vendedor)]
public class VendaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PedidoService _pedidos;
    private readonly UsuarioLogadoService _usuarioLogado;
    private readonly BuscaCompradorService _buscaComprador;

    public VendaController(ApplicationDbContext context, PedidoService pedidos, UsuarioLogadoService usuarioLogado, BuscaCompradorService buscaComprador)
    {
        _context = context;
        _pedidos = pedidos;
        _usuarioLogado = usuarioLogado;
        _buscaComprador = buscaComprador;
    }

    // GET: VENDA/Create
    public async Task<IActionResult> Create()
    {
        await CarregarProdutos();
        return View(new NovoPedidoViewModel { Linhas = Enumerable.Range(0, 8).Select(_ => new LinhaPedido()).ToList() });
    }

    // POST: VENDA/Create
    // A venda só é registrada se o comprador for um usuário padrão e houver estoque de todos os itens
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovoPedidoViewModel model)
    {
        var vendedor = await _usuarioLogado.ObterAsync(User);
        if (vendedor == null)
            return Forbid();

        // Acha o comprador pelo CPF ou código informado
        var comprador = await _buscaComprador.BuscarAsync(model.Comprador);
        if (comprador == null)
            ModelState.AddModelError(nameof(model.Comprador), "Comprador não encontrado. Informe o CPF ou o código de um usuário padrão.");
        else if (!BuscaCompradorService.EhPadrao(comprador))
            ModelState.AddModelError(nameof(model.Comprador), "Só é possível vender para usuários com a função Padrão.");

        // Ignora as linhas em branco do formulário
        var itens = model.Linhas
            .Where(l => l.ProdutoId > 0)
            .Select(l => (l.ProdutoId, Quantidade: l.Quantidade ?? 0))
            .ToList();

        if (ModelState.IsValid && comprador != null)
        {
            var (pedido, erro) = await _pedidos.RegistrarAsync(TipoPedido.Venda, comprador.UsuarioId, vendedor.UsuarioId, itens);
            if (erro == null)
            {
                TempData["Sucesso"] = $"Venda registrada: {pedido!.Itens.Count} produto(s), total {pedido.Total:C2}.";
                return RedirectToAction(nameof(Create));
            }
            ModelState.AddModelError(string.Empty, erro);
        }

        await CarregarProdutos();
        return View(model);
    }

    // Lista os produtos com estoque para o vendedor escolher
    private async Task CarregarProdutos()
    {
        // Os produtos vão inteiros (nome, preço e estoque): a tela monta as opções e calcula o total sem ida ao servidor
        var produtos = await _context.Produtos
            .Where(p => p.Estoque > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
        ViewBag.Produtos = produtos;
        ViewBag.ExigeComprador = true;
    }
}
