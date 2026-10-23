
// Necessário para usar o atributo [Authorize]
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
// Política de ADM (Politicas.Admin / FuncaoAdminId) definida em Authorization/AdminRequirement.cs
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using System.Security.Claims;

// Todas as ações exigem login; as de gerenciamento exigem ADM.
// Create fica liberado para qualquer usuário logado criar o próprio cadastro.
[Authorize]
public class UsuarioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    // Usado para verificar se o usuário logado é ADM
    private readonly IAuthorizationService _authorizationService;


    // Recebe o IAuthorizationService por injeção de dependência
    public UsuarioController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService)
    {
        _context = context;
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    // Só ADM pode dar a função ADM; enquanto não existir nenhum ADM, o primeiro cadastro pode
    private async Task<bool> PodeAtribuirAdmin()
    {
        var ehAdmin = (await _authorizationService.AuthorizeAsync(User, Politicas.Admin)).Succeeded;
        return ehAdmin || !await _context.Usuarios.AnyAsync(u => u.FuncaoId == Politicas.FuncaoAdminId);
    }

    // Monta o dropdown de funções; esconde a função ADM de quem não pode atribuí-la
    private async Task CarregarFuncoes()
    {
        var funcoes = _context.Funcoes.AsQueryable();
        if (!await PodeAtribuirAdmin())
            funcoes = funcoes.Where(f => f.FuncaoId != Politicas.FuncaoAdminId);

        ViewData["FuncaoId"] = new SelectList(await funcoes.ToListAsync(), "FuncaoId", "Name");
    }

    // GET: USUARIOS
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Index()
    {
        // Inclui a função para a lista mostrar o nome dela, e não só o código
        var usuarios = await _context.Usuarios.Include(u => u.Funcao).ToListAsync();
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
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
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
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction("Index", "Home");

        var identityUser = await _userManager.FindByIdAsync(userId);

        var usuario = new Usuario
        {
            Email = identityUser?.Email ?? ""
        };
        // Carrega as funções do banco para o dropdown
        // Recarrega o dropdown de funções (sem a função ADM para quem não pode atribuí-la)
        await CarregarFuncoes();
        return View(usuario);
    }

    // POST: USUARIOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UsuarioId,Name,Email,Phone,CPF,FuncaoId,Funcao,Password")] Usuario usuario)
    {
        // Impede que um usuário comum se cadastre como ADM
        if (usuario.FuncaoId == Politicas.FuncaoAdminId && !await PodeAtribuirAdmin())
        {
            ModelState.AddModelError("FuncaoId", "Apenas um administrador pode atribuir a função de administrador.");
        }

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
                // Recarrega o dropdown para a tela não voltar com a lista vazia
                await CarregarFuncoes();
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
                // Recarrega o dropdown para a tela não voltar com a lista vazia
                await CarregarFuncoes();
                return View(usuario);
            }
            _context.Add(usuario);
            await _context.SaveChangesAsync();

          

            return RedirectToAction("Index", "Home");
        }
        // Recarrega o dropdown de funções (sem a função ADM para quem não pode atribuí-la)
        await CarregarFuncoes();
        return View(usuario);
    }

    // GET: USUARIOS/Edit/5
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
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
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Edit(int? usuarioid, [Bind("UsuarioId,Name,Email,Phone,CPF,FuncaoId,Funcao,Password,AppUserId")] Usuario usuario)
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
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
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
    // Somente ADM pode acessar esta ação
    [Authorize(Policy = Politicas.Admin)]
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
