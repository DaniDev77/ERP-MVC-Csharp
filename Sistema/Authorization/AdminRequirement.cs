using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;

namespace Sistema.Authorization
{
    public static class Politicas
    {
        // Nome da política usada em [Authorize(Policy = Politicas.Admin)]
        public const string Admin = "Admin";

        // Usuários com esta FuncaoId controlam o sistema inteiro
        public const int FuncaoAdminId = 1;
    }

    public class AdminRequirement : IAuthorizationRequirement { }

    // Consulta o banco a cada requisição, então mudar a função de alguém vale na hora
    public class AdminHandler(ApplicationDbContext context) : AuthorizationHandler<AdminRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, AdminRequirement requirement)
        {
            var userId = authContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var appUserId))
                return;

            var ehAdmin = await context.Usuarios
                .AnyAsync(u => u.AppUserId == appUserId && u.FuncaoId == Politicas.FuncaoAdminId);

            if (ehAdmin)
                authContext.Succeed(requirement);
        }
    }
}
