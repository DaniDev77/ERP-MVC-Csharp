using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;

namespace Sistema.Authorization
{
    // Exige que a FuncaoId do usuário esteja na lista de funções permitidas
    public class FuncaoRequirement(params int[] funcoesPermitidas) : IAuthorizationRequirement
    {
        public int[] FuncoesPermitidas { get; } = funcoesPermitidas;
    }

    // Igual ao AdminHandler: consulta o banco a cada requisição
    public class FuncaoHandler(ApplicationDbContext context) : AuthorizationHandler<FuncaoRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, FuncaoRequirement requirement)
        {
            var userId = authContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var appUserId))
                return;

            // Verifica se a função do usuário logado está entre as permitidas pela política
            var permitido = await context.Usuarios
                .AnyAsync(u => u.AppUserId == appUserId && requirement.FuncoesPermitidas.Contains(u.FuncaoId));

            if (permitido)
                authContext.Succeed(requirement);
        }
    }
}
