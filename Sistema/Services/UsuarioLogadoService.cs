using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Descobre o registro de Usuario (tabela do sistema) do usuário logado
    public class UsuarioLogadoService(ApplicationDbContext context)
    {
        public async Task<Usuario?> ObterAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var appUserId))
                return null;

            // Inclui a função para o menu e o perfil mostrarem o nome dela
            return await context.Usuarios
                .Include(u => u.Funcao)
                .FirstOrDefaultAsync(u => u.AppUserId == appUserId);
        }
    }
}
