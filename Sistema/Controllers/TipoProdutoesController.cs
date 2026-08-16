
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;
using Sistema.Data;

public class TipoProdutoesController : Controller
{
    private readonly ApplicationDbContext _context;

    public TipoProdutoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: TIPOPRODUTOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.TiposProdutos.ToListAsync());
    }

    // GET: TIPOPRODUTOS/Details/5
    public async Task<IActionResult> Details(int? tipoprodutoid)
    {
        if (tipoprodutoid == null)
        {
            return NotFound();
        }

        var tipoproduto = await _context.TiposProdutos
            .FirstOrDefaultAsync(m => m.TipoProdutoId == tipoprodutoid);
        if (tipoproduto == null)
        {
            return NotFound();
        }

        return View(tipoproduto);
    }

    // GET: TIPOPRODUTOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TIPOPRODUTOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TipoProdutoId,TipoNome,TipoDescricao")] TipoProduto tipoproduto)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tipoproduto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tipoproduto);
    }

    // GET: TIPOPRODUTOS/Edit/5
    public async Task<IActionResult> Edit(int? tipoprodutoid)
    {
        if (tipoprodutoid == null)
        {
            return NotFound();
        }

        var tipoproduto = await _context.TiposProdutos.FindAsync(tipoprodutoid);
        if (tipoproduto == null)
        {
            return NotFound();
        }
        return View(tipoproduto);
    }

    // POST: TIPOPRODUTOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? tipoprodutoid, [Bind("TipoProdutoId,TipoNome,TipoDescricao")] TipoProduto tipoproduto)
    {
        if (tipoprodutoid != tipoproduto.TipoProdutoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tipoproduto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoProdutoExists(tipoproduto.TipoProdutoId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(tipoproduto);
    }

    // GET: TIPOPRODUTOS/Delete/5
    public async Task<IActionResult> Delete(int? tipoprodutoid)
    {
        if (tipoprodutoid == null)
        {
            return NotFound();
        }

        var tipoproduto = await _context.TiposProdutos
            .FirstOrDefaultAsync(m => m.TipoProdutoId == tipoprodutoid);
        if (tipoproduto == null)
        {
            return NotFound();
        }

        return View(tipoproduto);
    }

    // POST: TIPOPRODUTOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? tipoprodutoid)
    {
        var tipoproduto = await _context.TiposProdutos.FindAsync(tipoprodutoid);
        if (tipoproduto != null)
        {
            _context.TiposProdutos.Remove(tipoproduto);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TipoProdutoExists(int? tipoprodutoid)
    {
        return _context.TiposProdutos.Any(e => e.TipoProdutoId == tipoprodutoid);
    }
}
