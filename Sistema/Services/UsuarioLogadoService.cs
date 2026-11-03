using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Descobre o registro de Usuario (tabela do sistema) do usuário logado
    public class UsuarioLogadoService(ApplicationDbContext context)
    {
        // Novo (direção D, B6): guarda o usuário da requisição (o serviço é Scoped). O _LoginPartial e a conta da
        // barra do topo no celular leem o mesmo registro sem uma segunda consulta
        private (Guid Id, Usuario? Usuario)? cache;

        public async Task<Usuario?> ObterAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var appUserId))
                return null;
            if (cache is { } c && c.Id == appUserId)
                return c.Usuario;

            // Inclui a função para o menu e o perfil mostrarem o nome dela
            var usuario = await context.Usuarios
                .Include(u => u.Funcao)
                .FirstOrDefaultAsync(u => u.AppUserId == appUserId);
            cache = (appUserId, usuario);
            return usuario;
        }
    }
}
