using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;
using System.Security.Claims;

// Gestão de usuários. Só o ADM lista, edita, exclui e define a função (vendedor ou padrão).
// Qualquer conta logada pode completar o próprio cadastro, mas sem função: quem libera é o ADM.
[Authorize]
public class UsuarioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IConfiguration _configuracao;

    public UsuarioController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService, IConfiguration configuracao)
    {
        _context = context;
        _userManager = userManager;
        _authorizationService = authorizationService;
        _configuracao = configuracao;
    }

    // GET: USUARIO
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Funcao)
            .OrderBy(u => u.Name)
            .ToListAsync();
        return View(usuarios);
    }

    // GET: USUARIO/Details/5
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Details(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Funcao)
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // GET: USUARIO/Create
    // Quem não é ADM cria o próprio cadastro sem função; o ADM vê o seletor de função
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction("Index", "Home");

        // Quem não é ADM já tendo cadastro não cria outro; o ADM cadastra outras pessoas à vontade
        if (!await EhAdminAsync() && await _context.Usuarios.AnyAsync(u => u.AppUserId == Guid.Parse(userId)))
            return RedirectToAction("Index", "Perfil");

        var identityUser = await _userManager.FindByIdAsync(userId);
        var usuario = new Usuario { Email = identityUser?.Email ?? "" };

        await PrepararFormulario(usuario);
        return View(usuario);
    }

    // POST: USUARIO/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Email,Phone,CPF,FuncaoId")] Usuario usuario)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId, out var appUserId))
            return Forbid();

        // Quem pode escolher a função: o ADM. Se ainda não existe nenhum ADM, só a conta
        // cujo e-mail está em Sistema:EmailPrimeiroAdmin vira administrador (configuração inicial).
        var ehAdmin = await EhAdminAsync();
        var primeiroAdmin = !ehAdmin && await PodeSerPrimeiroAdminAsync();

        // Quem não é ADM tem um cadastro só para a própria conta
        if (!ehAdmin && await _context.Usuarios.AnyAsync(u => u.AppUserId == appUserId))
        {
            ModelState.AddModelError(string.Empty, "Esta conta já tem cadastro de usuário.");
            await PrepararFormulario(usuario);
            return View(usuario);
        }

        if (primeiroAdmin)
        {
            usuario.FuncaoId = Politicas.FuncaoAdminId;
        }
        else if (!ehAdmin)
        {
            // Cadastro próprio nunca recebe função: fica aguardando o ADM liberar
            usuario.FuncaoId = null;
        }
        else if (usuario.FuncaoId == null)
        {
            ModelState.AddModelError("FuncaoId", "Escolha a função do usuário.");
        }

        if (await _context.Usuarios.AnyAsync(u => u.CPF == usuario.CPF))
        {
            ModelState.AddModelError("CPF", "Já existe um usuário cadastrado com este CPF.");
        }

        if (!ModelState.IsValid)
        {
            await PrepararFormulario(usuario);
            return View(usuario);
        }

        // Cadastro feito pelo ADM fica sem conta de acesso: a conta é vinculada depois, pelo e-mail, na edição.
        // Assim o ADM nunca amarra um cadastro de terceiro à própria conta.
        usuario.AppUserId = ehAdmin ? null : appUserId;
        // A senha de acesso fica só no Identity (com hash). O campo legado não recebe valor.
        usuario.Password = string.Empty;
        _context.Add(usuario);
        await _context.SaveChangesAsync();

        if (ehAdmin)
        {
            TempData["Sucesso"] = "Usuário cadastrado. Para ele acessar, vincule a conta dele pelo e-mail na edição.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Sucesso"] = primeiroAdmin
            ? "Você é o administrador do sistema."
            : "Cadastro enviado. Um administrador vai liberar sua função para você começar.";
        return RedirectToAction("Index", "Perfil");
    }

    // POST: USUARIO/Vincular/5 — o ADM liga o cadastro à conta de acesso que tem aquele e-mail
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Vincular(int usuarioid, string email)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        var conta = string.IsNullOrWhiteSpace(email) ? null : await _userManager.FindByEmailAsync(email.Trim());
        if (conta == null)
        {
            TempData["Erro"] = "Nenhuma conta de acesso tem esse e-mail. Peça para a pessoa criar a conta primeiro.";
            return RedirectToAction(nameof(Edit), new { usuarioid });
        }

        // Uma conta só pode estar ligada a um cadastro
        if (await _context.Usuarios.AnyAsync(u => u.AppUserId == Guid.Parse(conta.Id) && u.UsuarioId != usuarioid))
        {
            TempData["Erro"] = "Essa conta já está vinculada a outro cadastro.";
            return RedirectToAction(nameof(Edit), new { usuarioid });
        }

        usuario.AppUserId = Guid.Parse(conta.Id);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Conta vinculada. A pessoa já pode entrar com o e-mail " + conta.Email + ".";
        return RedirectToAction(nameof(Edit), new { usuarioid });
    }

    // GET: USUARIO/Edit/5
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

        await PrepararFormulario(usuario);
        return View(usuario);
    }

    // POST: USUARIO/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Edit(int? usuarioid, [Bind("UsuarioId,Name,Email,Phone,CPF,FuncaoId")] Usuario usuario)
    {
        if (usuarioid != usuario.UsuarioId)
        {
            return NotFound();
        }

        // Carrega o registro real: só os campos do formulário mudam (o vínculo com a conta fica intacto)
        var existente = await _context.Usuarios.FindAsync(usuario.UsuarioId);
        if (existente == null)
        {
            return NotFound();
        }

        if (usuario.FuncaoId == null)
        {
            ModelState.AddModelError("FuncaoId", "Escolha a função do usuário.");
        }

        if (await _context.Usuarios.AnyAsync(u => u.CPF == usuario.CPF && u.UsuarioId != usuario.UsuarioId))
        {
            ModelState.AddModelError("CPF", "Já existe um usuário cadastrado com este CPF.");
        }

        // Ninguém tira a própria função ADM: senão pode ficar sem acesso ao sistema
        var euMesmo = existente.AppUserId == Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
        if (euMesmo && usuario.FuncaoId != existente.FuncaoId)
        {
            ModelState.AddModelError("FuncaoId", "Você não pode trocar a própria função.");
        }

        // Não tira a função ADM do último administrador
        if (existente.FuncaoId == Politicas.FuncaoAdminId && usuario.FuncaoId != Politicas.FuncaoAdminId
            && await ContarAdministradoresAsync() <= 1)
        {
            ModelState.AddModelError("FuncaoId", "Este é o último administrador: deixe outro ADM antes de trocar esta função.");
        }

        if (!ModelState.IsValid)
        {
            await PrepararFormulario(usuario);
            return View(usuario);
        }

        existente.Name = usuario.Name;
        existente.Email = usuario.Email;
        existente.Phone = usuario.Phone;
        existente.CPF = usuario.CPF;
        existente.FuncaoId = usuario.FuncaoId;

        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Dados do usuário atualizados.";
        return RedirectToAction(nameof(Index));
    }

    // GET: USUARIO/Delete/5
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> Delete(int? usuarioid)
    {
        if (usuarioid == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Funcao)
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    // POST: USUARIO/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int? usuarioid)
    {
        var usuario = await _context.Usuarios.Include(u => u.Funcao).FirstOrDefaultAsync(u => u.UsuarioId == usuarioid);
        if (usuario == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Ninguém apaga o próprio cadastro nem o último ADM (o sistema ficaria sem administrador)
        var euMesmo = usuario.AppUserId == Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
        if (euMesmo)
        {
            ModelState.AddModelError(string.Empty, "Você não pode excluir o próprio cadastro.");
            return View("Delete", usuario);
        }
        if (usuario.FuncaoId == Politicas.FuncaoAdminId && await ContarAdministradoresAsync() <= 1)
        {
            ModelState.AddModelError(string.Empty, "Este é o último administrador. Crie ou libere outro ADM antes de excluir.");
            return View("Delete", usuario);
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Usuário excluído.";
        return RedirectToAction(nameof(Index));
    }

    // Monta o seletor de função conforme quem está logado; quem não é ADM não vê o seletor
    private async Task PrepararFormulario(Usuario usuario)
    {
        var ehAdmin = await EhAdminAsync();
        ViewBag.PodeDefinirFuncao = ehAdmin || (usuario.UsuarioId == 0 && await PodeSerPrimeiroAdminAsync());
        ViewBag.EhNovo = usuario.UsuarioId == 0;

        // E-mail da conta de acesso ligada a este cadastro, para o ADM conferir o vínculo
        ViewBag.ContaEmail = usuario.AppUserId == null
            ? null
            : (await _userManager.FindByIdAsync(usuario.AppUserId.ToString()!))?.Email;

        var funcoes = await _context.Funcoes.OrderBy(f => f.FuncaoId).ToListAsync();
        ViewData["FuncaoId"] = new SelectList(funcoes, "FuncaoId", "Name", usuario.FuncaoId);
    }

    private async Task<bool> EhAdminAsync()
    {
        return (await _authorizationService.AuthorizeAsync(User, Politicas.Admin)).Succeeded;
    }

    private async Task<int> ContarAdministradoresAsync()
    {
        return await _context.Usuarios.CountAsync(u => u.FuncaoId == Politicas.FuncaoAdminId);
    }

    // Bootstrap: só quando não existe nenhum ADM e o e-mail da conta é o configurado em appsettings
    private async Task<bool> PodeSerPrimeiroAdminAsync()
    {
        var emailConfigurado = _configuracao["Sistema:EmailPrimeiroAdmin"];
        if (string.IsNullOrWhiteSpace(emailConfigurado))
            return false;
        if (await ContarAdministradoresAsync() > 0)
            return false;

        var identityUser = await _userManager.GetUserAsync(User);
        return string.Equals(identityUser?.Email, emailConfigurado, StringComparison.OrdinalIgnoreCase);
    }
}
