
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;
using Sistema.Data;

public class FonecedorController : Controller
{
    private readonly ApplicationDbContext _context;

    public FonecedorController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: FONECEDORS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Fornecedores.ToListAsync());
    }

    // GET: FONECEDORS/Details/5
    public async Task<IActionResult> Details(System.Guid? fonecedorid)
    {
        if (fonecedorid == null)
        {
            return NotFound();
        }

        var fonecedor = await _context.Fornecedores
            .FirstOrDefaultAsync(m => m.FonecedorId == fonecedorid);
        if (fonecedor == null)
        {
            return NotFound();
        }

        return View(fonecedor);
    }

    // GET: FONECEDORS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FONECEDORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FonecedorId,FonecedorNome,FonecedorDescricao,CNPJ,FonecedorTelefone,FonecedorEmail")] Fonecedor fonecedor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(fonecedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(fonecedor);
    }

    // GET: FONECEDORS/Edit/5
    public async Task<IActionResult> Edit(System.Guid? fonecedorid)
    {
        if (fonecedorid == null)
        {
            return NotFound();
        }

        var fonecedor = await _context.Fornecedores.FindAsync(fonecedorid);
        if (fonecedor == null)
        {
            return NotFound();
        }
        return View(fonecedor);
    }

    // POST: FONECEDORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(System.Guid? fonecedorid, [Bind("FonecedorId,FonecedorNome,FonecedorDescricao,CNPJ,FonecedorTelefone,FonecedorEmail")] Fonecedor fonecedor)
    {
        if (fonecedorid != fonecedor.FonecedorId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(fonecedor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FonecedorExists(fonecedor.FonecedorId))
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
        return View(fonecedor);
    }

    // GET: FONECEDORS/Delete/5
    public async Task<IActionResult> Delete(System.Guid? fonecedorid)
    {
        if (fonecedorid == null)
        {
            return NotFound();
        }

        var fonecedor = await _context.Fornecedores
            .FirstOrDefaultAsync(m => m.FonecedorId == fonecedorid);
        if (fonecedor == null)
        {
            return NotFound();
        }

        return View(fonecedor);
    }

    // POST: FONECEDORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(System.Guid? fonecedorid)
    {
        var fonecedor = await _context.Fornecedores.FindAsync(fonecedorid);
        if (fonecedor != null)
        {
            _context.Fornecedores.Remove(fonecedor);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FonecedorExists(System.Guid? fonecedorid)
    {
        return _context.Fornecedores.Any(e => e.FonecedorId == fonecedorid);
    }
}
