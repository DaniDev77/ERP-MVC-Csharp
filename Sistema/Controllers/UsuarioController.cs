
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;
using System.Security.Claims;

public class UsuarioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public UsuarioController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: USUARIOS
    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        var rolesPorUsuario = new Dictionary<int, string>();

        foreach (var usuario in usuarios)
        {
            if (usuario.AppUserId.HasValue)
            {
                var identityUser = await _userManager.FindByIdAsync(usuario.AppUserId.ToString());
                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    rolesPorUsuario[usuario.UsuarioId] = roles.FirstOrDefault() ?? "Nenhuma";
                }
                else
                {
                    rolesPorUsuario[usuario.UsuarioId] = "Nenhuma";
                }
            }
            else
            {
                rolesPorUsuario[usuario.UsuarioId] = "Nenhuma";
            }
        }

        ViewBag.RolesPorUsuario = rolesPorUsuario;
        return View(usuarios);
    }

    // GET: USUARIOS/Details/5
    public async Task<IActionResult> Details(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // GET: USUARIOS/Create
    public IActionResult Create()
    {
        ViewData["FuncaoId"] = new SelectList(_context.Funcoes, "FuncaoId", "Name");
        ViewData["AppUserId"] = new SelectList(
          _context.Users,
          "Id",
          "UserName"
      );

        return View();
    }

    // POST: USUARIOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UsuarioId,Name,Email,Phone,CPF,FuncaoId,Funcao,Password,AppUserId,IdentityUser")] Usuario usuario)
    {
        if (ModelState.IsValid)
        { 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return NotFound();

            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.AppUserId == Guid.Parse(userId));
            if (existingUser != null)
            {
                ModelState.AddModelError("AppUserId", "E-mail já cadastrado.");
                return View(usuario);
            }

            usuario.AppUserId = Guid.Parse(userId);

            var identityUser = await _context.Users.FindAsync(userId);

            if (identityUser != null)
            {
                usuario.IdentityUser = identityUser;
            }
            else
            {
                ModelState.AddModelError("AppUserId", "Usuário não encontrado.");
                return View(usuario);
            }
            _context.Add(usuario);
            await _context.SaveChangesAsync();

            // Adiciona o usuário à role "Aluno"
           // await _userManager.AddToRoleAsync(identityUser, "Aluno");

            return RedirectToAction("Index", "Home");
        }
        return View(usuario);
    }

    // GET: USUARIOS/Edit/5
    public async Task<IActionResult> Edit(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios.FindAsync(usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    // POST: USUARIOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? usuarioid, [Bind("UsuarioId,Name,Email,Phone,CPF,FuncaoId,Funcao,Password,AppUserId,IdentityUser")] Usuario usuario)
    {
        if (usuarioid != usuario.UsuarioId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(usuario.UsuarioId))
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
        return View(usuario);
    }

    // GET: USUARIOS/Delete/5
    public async Task<IActionResult> Delete(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // POST: USUARIOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? usuarioid)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioid);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UsuarioExists(int? usuarioid)
    {
        return _context.Usuarios.Any(e => e.UsuarioId == usuarioid);
    }
}
