using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

// Compras: usuário padrão (e ADM) compra produtos para si mesmo.
// Alterado: o envio agora cria um PEDIDO pendente (PedidoService.CriarAsync). A Compra só nasce quando o ADM
// confirma em Pedido/Index. A rota (Compra/Create) e os campos do formulário continuam os mesmos
[Authorize(Policy = Politicas.Padrao)]
public class CompraController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PedidoService _pedidos;
    private readonly UsuarioLogadoService _usuarioLogado;
    private readonly MunicipioService _municipios;

    public CompraController(ApplicationDbContext context, PedidoService pedidos, UsuarioLogadoService usuarioLogado, MunicipioService municipios)
    {
        _context = context;
        _pedidos = pedidos;
        _usuarioLogado = usuarioLogado;
        _municipios = municipios;
    }

    // GET: COMPRA/Create
    public async Task<IActionResult> Create()
    {
        var comprador = await _usuarioLogado.ObterAsync(User);
        await CarregarProdutos(comprador);
        return View(new NovoPedidoViewModel { Linhas = Enumerable.Range(0, 8).Select(_ => new LinhaPedido()).ToList() });
    }

    // POST: COMPRA/Create
    // O comprador é sempre o usuário logado; o pedido fica pendente até o ADM confirmar
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
            .Select(l => (l.ProdutoId, Quantidade: l.Quantidade ?? 0))
            .ToList();

        // Alterado: cria o pedido (sem baixar estoque) em vez de registrar a compra direto
        var (pedido, erro) = await _pedidos.CriarAsync(comprador, itens);
        if (erro == null)
        {
            TempData["Sucesso"] = $"Pedido nº {pedido!.PedidoId} enviado: {pedido.Itens.Count} produto(s), total {pedido.Total:C2}. Ele fica pendente até a loja confirmar.";
            // Alterado: leva para "Meus pedidos", onde o cliente acompanha o status
            return RedirectToAction("Meus", "Pedido");
        }
        ModelState.AddModelError(string.Empty, erro);

        await CarregarProdutos(comprador);
        return View(model);
    }

    // Lista os produtos com estoque para o usuário escolher
    private async Task CarregarProdutos(Usuario? comprador)
    {
        // Os produtos vão inteiros (nome, preço e estoque): a tela monta as opções e calcula o total sem ida ao servidor
        var produtos = await _context.Produtos
            .Where(p => p.Estoque > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
        ViewBag.Produtos = produtos;
        ViewBag.ExigeComprador = false;

        // Novo: se o cliente ainda não respondeu sobre a localização, a tela mostra o pedido de consentimento (LGPD).
        // Quem já informou ou recusou não é perguntado de novo
        if (comprador != null && comprador.LocalizacaoRespondidaEm == null)
        {
            ViewBag.Localizacao = new LocalizacaoViewModel
            {
                ReturnUrl = Url.Action(nameof(Create)),
                Ufs = MunicipioService.Ufs
            };
        }
    }
}
