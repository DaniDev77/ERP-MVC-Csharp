using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;
// Política de ADM (Politicas.Admin) definida em Authorization/AdminRequirement.cs
using Sistema.Authorization;

// Cadastro de produtos: controller inteiro restrito a ADM
[Authorize(Policy = Politicas.Admin)]
public class ProdutoController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProdutoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PRODUTO?filtro=baixo|zerado
    // Inclui fornecedor e tipo para mostrar os nomes na lista.
    // Os cartões de contagem da tela usam o filtro: "baixo" = entre 1 e o mínimo, "zerado" = sem estoque
    public async Task<IActionResult> Index(string? filtro)
    {
        // Contagens para os cartões, sempre do banco inteiro (não só do filtro aplicado)
        ViewBag.Total = await _context.Produtos.CountAsync();
        ViewBag.Zerados = await _context.Produtos.CountAsync(p => p.Estoque == 0);
        ViewBag.Baixos = await _context.Produtos.CountAsync(p => p.Estoque > 0 && p.Estoque <= Produto.EstoqueMinimo);
        ViewBag.Filtro = filtro;

        var consulta = _context.Produtos
            .Include(p => p.Fornecedor)
            .Include(p => p.TipoProduto)
            .AsQueryable();

        consulta = filtro switch
        {
            "baixo" => consulta.Where(p => p.Estoque > 0 && p.Estoque <= Produto.EstoqueMinimo),
            "zerado" => consulta.Where(p => p.Estoque == 0),
            _ => consulta
        };

        var produtos = await consulta.OrderBy(p => p.Name).ToListAsync();
        return View(produtos);
    }

    // GET: PRODUTO/EstoqueBaixo
    // Lista os produtos com estoque igual ou abaixo do mínimo, para o ADM repor
    public async Task<IActionResult> EstoqueBaixo()
    {
        var produtos = await _context.Produtos
            .Include(p => p.Fornecedor)
            .Include(p => p.TipoProduto)
            .Where(p => p.Estoque <= Produto.EstoqueMinimo)
            .OrderBy(p => p.Estoque)
            .ToListAsync();
        return View(produtos);
    }

    // GET: PRODUTO/Create
    public async Task<IActionResult> Create()
    {
        await CarregarListas();
        return View();
    }

    // POST: PRODUTO/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProdutoId,Name,Descricao,Preco,Estoque,FornecedorId,TipoProdutoId")] Produto produto)
    {
        if (ModelState.IsValid)
        {
            _context.Add(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await CarregarListas();
        return View(produto);
    }

    // GET: PRODUTO/Edit/5
    // Usado também para repor o estoque
    // Alterado (HF-005): recebe returnUrl para voltar à tela de origem (ex.: Estoque baixo) depois de salvar
    public async Task<IActionResult> Edit(int? id, string? returnUrl)
    {
        if (id == null)
        {
            return NotFound();
        }

        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }
        // Novo (HF-005): vai num hidden do form, porque o <form asp-action> não preserva a querystring
        ViewData["ReturnUrl"] = returnUrl;
        await CarregarListas();
        return View(produto);
    }

    // POST: PRODUTO/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Alterado (HF-005): RowVersion entra no Bind (vem do hidden do form) para detectar edição concorrente
    public async Task<IActionResult> Edit(int id, [Bind("ProdutoId,Name,Descricao,Preco,Estoque,FornecedorId,TipoProdutoId,RowVersion")] Produto produto, string? returnUrl)
    {
        if (id != produto.ProdutoId)
        {
            return NotFound();
        }

        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            await CarregarListas();
            return View(produto);
        }

        // Corrigido (HF-005): o Update() sem RowVersion gerava "WHERE [RowVersion] IS NULL" e dava erro 500 em toda edição
        var atual = await _context.Produtos.FindAsync(id);
        if (atual == null)
        {
            return NotFound();
        }
        // O original é o RowVersion que o form carregou: se uma venda mexeu no produto depois, o UPDATE não acha a linha
        _context.Entry(atual).Property(p => p.RowVersion).OriginalValue = produto.RowVersion;
        // Copia só os campos editáveis (o SET passa a ter só o que mudou)
        atual.Name = produto.Name;
        atual.Descricao = produto.Descricao;
        atual.Preco = produto.Preco;
        atual.Estoque = produto.Estoque;
        atual.FornecedorId = produto.FornecedorId;
        atual.TipoProdutoId = produto.TipoProdutoId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var banco = await ex.Entries.Single().GetDatabaseValuesAsync();
            // Produto excluído por outra pessoa no meio da edição
            if (banco == null)
            {
                TempData["Erro"] = $"{produto.Name} foi excluído enquanto você editava.";
                return RedirectToAction(nameof(Index));
            }
            var estoqueAgora = (int)banco[nameof(Produto.Estoque)]!;
            // Sem o Remove, o hidden volta com o RowVersion antigo (o ModelState vence o modelo) e todo Salvar daria conflito
            ModelState.Remove(nameof(Produto.RowVersion));
            produto.RowVersion = (byte[]?)banco[nameof(Produto.RowVersion)];
            // O chip do cabeçalho mostra o estoque atual; o campo continua com o que o ADM digitou
            produto.Estoque = estoqueAgora;
            ModelState.AddModelError(nameof(Produto.Estoque), $"O estoque mudou enquanto você editava (agora: {estoqueAgora} un.). Confira e salve de novo.");
            // Recarrega fornecedores e tipos, senão os selects voltam vazios
            await CarregarListas();
            return View(produto);
        }

        TempData["Sucesso"] = $"{atual.Name} atualizado.";
        // Só volta para endereço do próprio sistema (evita open redirect)
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    // GET: PRODUTO/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var produto = await _context.Produtos
            .Include(p => p.Fornecedor)
            .Include(p => p.TipoProduto)
            .FirstOrDefaultAsync(m => m.ProdutoId == id);
        if (produto == null)
        {
            return NotFound();
        }

        return View(produto);
    }

    // POST: PRODUTO/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto != null)
        {
            // Novo: produto que aparece em pedido, compra ou venda não pode ser apagado (o histórico depende dele).
            // Antes o banco recusava e a tela mostrava erro 500
            var emUso = await _context.PedidoItens.AnyAsync(i => i.ProdutoId == id)
                || await _context.CompraItens.AnyAsync(i => i.ProdutoId == id)
                || await _context.VendaItens.AnyAsync(i => i.ProdutoId == id);
            if (emUso)
            {
                TempData["Erro"] = $"{produto.Name} está em pedidos, compras ou vendas e não pode ser excluído. Zere o estoque para tirá-lo do catálogo.";
                return RedirectToAction(nameof(Index));
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // Monta os dropdowns de fornecedor e tipo de produto
    private async Task CarregarListas()
    {
        // Usa ViewBag (e não ViewData com string) para a view ler sem aspas aninhadas
        ViewBag.Fornecedores = new SelectList(await _context.Fornecedores.OrderBy(f => f.FonecedorNome).ToListAsync(), "FonecedorId", "FonecedorNome");
        ViewBag.Tipos = new SelectList(await _context.TiposProdutos.OrderBy(t => t.TipoNome).ToListAsync(), "TipoProdutoId", "TipoNome");
    }

    private bool ProdutoExists(int id)
    {
        return _context.Produtos.Any(e => e.ProdutoId == id);
    }
}
