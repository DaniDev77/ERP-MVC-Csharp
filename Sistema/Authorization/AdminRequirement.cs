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

        // Funções de venda e compra, com os ids já cadastrados na tabela Funcao ("VENDEDOR" e "USUARIO PADRÃO")
        public const int FuncaoVendedorId = 2;
        public const int FuncaoPadraoId = 1002;

        // Nomes das políticas de venda, compra e desempenho do vendedor
        // ADM passa em todas as políticas de venda e compra
        public const string Vendedor = "Vendedor";
        public const string Padrao = "Padrao";
        public const string SomenteVendedor = "SomenteVendedor";
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
