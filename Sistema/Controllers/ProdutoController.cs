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

    // GET: PRODUTO
    // Inclui fornecedor e tipo para mostrar os nomes na lista
    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produtos
            .Include(p => p.Fornecedor)
            .Include(p => p.TipoProduto)
            .OrderBy(p => p.Name)
            .ToListAsync();
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
    public async Task<IActionResult> Edit(int? id)
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
        await CarregarListas();
        return View(produto);
    }

    // POST: PRODUTO/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProdutoId,Name,Descricao,Preco,Estoque,FornecedorId,TipoProdutoId")] Produto produto)
    {
        if (id != produto.ProdutoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(produto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(produto.ProdutoId))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await CarregarListas();
        return View(produto);
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
