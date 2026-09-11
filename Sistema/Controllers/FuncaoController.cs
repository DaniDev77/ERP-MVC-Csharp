
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;
using Sistema.Data;

public class FuncaoController : Controller
{
    private readonly ApplicationDbContext _context;

    public FuncaoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: FUNCAOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Funcoes.ToListAsync());
    }

    // GET: FUNCAOS/Details/5
    public async Task<IActionResult> Details(int? funcaoid)
    {
        if (funcaoid == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes
            .FirstOrDefaultAsync(m => m.FuncaoId == funcaoid);
        if (funcao == null)
        {
            return NotFound();
        }

        return View(funcao);
    }

    // GET: FUNCAOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FUNCAOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FuncaoId,Name,Descricao")] Funcao funcao)
    {
        if (ModelState.IsValid)
        {
            _context.Add(funcao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(funcao);
    }

    // GET: FUNCAOS/Edit/5
    public async Task<IActionResult> Edit(int? funcaoid)
    {
        if (funcaoid == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes.FindAsync(funcaoid);
        if (funcao == null)
        {
            return NotFound();
        }
        return View(funcao);
    }

    // POST: FUNCAOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? funcaoid, [Bind("FuncaoId,Name,Descricao")] Funcao funcao)
    {
        if (funcaoid != funcao.FuncaoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(funcao);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuncaoExists(funcao.FuncaoId))
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
        return View(funcao);
    }

    // GET: FUNCAOS/Delete/5
    public async Task<IActionResult> Delete(int? funcaoid)
    {
        if (funcaoid == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes
            .FirstOrDefaultAsync(m => m.FuncaoId == funcaoid);
        if (funcao == null)
        {
            return NotFound();
        }

        return View(funcao);
    }

    // POST: FUNCAOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? funcaoid)
    {
        var funcao = await _context.Funcoes.FindAsync(funcaoid);
        if (funcao != null)
        {
            _context.Funcoes.Remove(funcao);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FuncaoExists(int? funcaoid)
    {
        return _context.Funcoes.Any(e => e.FuncaoId == funcaoid);
    }
}
