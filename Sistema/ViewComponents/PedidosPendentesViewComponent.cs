using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.ViewComponents
{
    // Novo (HF-008): item "Pedidos" do menu do ADM com a quantidade de pedidos aguardando confirmação
    public class PedidosPendentesViewComponent(ApplicationDbContext context, IAuthorizationService authorizationService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Defesa extra: mesmo chamado dentro de @if (podeAdmin), só o ADM recebe o item
            if (!(await authorizationService.AuthorizeAsync(UserClaimsPrincipal, Politicas.Admin)).Succeeded)
                return Content(string.Empty);

            // Só a contagem, de todos os pendentes, sem período (índice Status + DataHora); não carrega os pedidos.
            // É o mesmo número da aba "Pendentes" com periodo=todos e do aviso da Home
            var pendentes = await context.Pedidos.CountAsync(p => p.Status == StatusPedido.Pendente);
            return View(pendentes);
        }
    }
}
