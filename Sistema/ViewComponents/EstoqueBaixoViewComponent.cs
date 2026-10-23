using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.ViewComponents
{
    // Aviso no menu com a quantidade de produtos com estoque baixo; só aparece para o ADM
    public class EstoqueBaixoViewComponent(ApplicationDbContext context, IAuthorizationService authorizationService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!(await authorizationService.AuthorizeAsync(UserClaimsPrincipal, Politicas.Admin)).Succeeded)
                return Content(string.Empty);

            var produtos = await context.Produtos
                .Where(p => p.Estoque <= Produto.EstoqueMinimo)
                .OrderBy(p => p.Estoque)
                .ToListAsync();

            return View(produtos);
        }
    }
}
