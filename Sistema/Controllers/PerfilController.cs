using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Services;

// Perfil de quem está logado: mostra os dados da própria conta (qualquer usuário logado vê só o seu)
[Authorize]
public class PerfilController(UsuarioLogadoService usuarioLogado) : Controller
{
    // GET: PERFIL
    // Usuario é null quando a conta ainda não completou o cadastro; a view mostra o convite para completar
    public async Task<IActionResult> Index()
    {
        var usuario = await usuarioLogado.ObterAsync(User);
        return View(usuario);
    }
}
