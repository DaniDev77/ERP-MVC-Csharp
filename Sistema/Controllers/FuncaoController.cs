
// Necessário para usar o atributo [Authorize]
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;
using Sistema.Data;
// Política de ADM (Politicas.Admin / FuncaoAdminId) definida em Authorization/AdminRequirement.cs
using Sistema.Authorization;

// Controller inteiro restrito a ADM (usuários com FuncaoId = 1)
[Authorize(Policy = Politicas.Admin)]
public class FuncaoController : Controller
{
    private readonly ApplicationDbContext _context;

    // Funções que o próprio sistema usa nas políticas de acesso: não podem ser excluídas
    private static readonly int[] FuncoesReservadas = { Politicas.FuncaoAdminId, Politicas.FuncaoVendedorId, Politicas.FuncaoPadraoId };

    public FuncaoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: FUNCAO
    public async Task<IActionResult> Index()
    {
        return View(await _context.Funcoes.OrderBy(f => f.FuncaoId).ToListAsync());
    }

    // GET: FUNCAO/Details/5
    // O parâmetro se chama "id" porque é esse o nome usado pela rota padrão (/{controller}/{action}/{id?})
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes.FirstOrDefaultAsync(m => m.FuncaoId == id);
        if (funcao == null)
        {
            return NotFound();
        }

        return View(funcao);
    }

    // GET: FUNCAO/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FUNCAO/Create
    // O Id não é lido do formulário: quem gera o código é o banco
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Descricao")] Funcao funcao)
    {
        if (ModelState.IsValid)
        {
            _context.Add(funcao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(funcao);
    }

    // GET: FUNCAO/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes.FindAsync(id);
        if (funcao == null)
        {
            return NotFound();
        }
        return View(funcao);
    }

    // POST: FUNCAO/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("FuncaoId,Name,Descricao")] Funcao funcao)
    {
        // A URL e o campo oculto precisam apontar para a mesma função
        if (id != funcao.FuncaoId)
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
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(funcao);
    }

    // GET: FUNCAO/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var funcao = await _context.Funcoes.FirstOrDefaultAsync(m => m.FuncaoId == id);
        if (funcao == null)
        {
            return NotFound();
        }

        return View(funcao);
    }

    // POST: FUNCAO/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var funcao = await _context.Funcoes.FindAsync(id);
        if (funcao == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Função usada pelo sistema: apagá-la quebraria as regras de venda, compra e ADM
        if (FuncoesReservadas.Contains(funcao.FuncaoId))
        {
            ModelState.AddModelError(string.Empty, "Esta função é usada pelo sistema e não pode ser excluída.");
            return View("Delete", funcao);
        }

        // Com usuários nesta função, a exclusão apagaria os usuários em cascata: bloqueia antes
        if (await _context.Usuarios.AnyAsync(u => u.FuncaoId == funcao.FuncaoId))
        {
            ModelState.AddModelError(string.Empty, "Existem usuários com esta função. Troque a função deles antes de excluir.");
            return View("Delete", funcao);
        }

        _context.Funcoes.Remove(funcao);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FuncaoExists(int? funcaoid)
    {
        return _context.Funcoes.Any(e => e.FuncaoId == funcaoid);
    }
}
