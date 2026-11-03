using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.ViewComponents
{
    // Novo (HF-008): item "Pedidos" do menu do ADM com a quantidade de pedidos aguardando confirmação.
    // Alterado (direção D, B1): o mesmo item serve o menu lateral e a aba "Pedidos" do rodapé do celular.
    // Autorização e contagem rodam uma vez por requisição; a segunda chamada lê o resultado guardado
    public class PedidosPendentesViewComponent(ApplicationDbContext context, IAuthorizationService authorizationService) : ViewComponent
    {
        // Chave privada do cache da requisição (objeto, para não colidir com chaves de texto de outros códigos)
        private static readonly object ChaveCache = new();

        // modo "menu" (padrão, Default.cshtml) ou "aba" (Aba.cshtml); qualquer outro valor cai no menu
        public async Task<IViewComponentResult> InvokeAsync(string modo = "menu")
        {
            var pendentes = await ContarAsync();
            // null = não é ADM: nada aparece, nos dois modos (defesa extra além do @if (podeAdmin))
            if (pendentes == null)
                return Content(string.Empty);
            return View(modo == "aba" ? "Aba" : "Default", pendentes.Value);
        }

        private async Task<int?> ContarAsync()
        {
            if (HttpContext.Items.TryGetValue(ChaveCache, out var guardado))
                return (int?)guardado;

            int? resultado = null;
            if ((await authorizationService.AuthorizeAsync(UserClaimsPrincipal, Politicas.Admin)).Succeeded)
                // Todos os pendentes, sem período (índice Status + DataHora): o mesmo número da aba "Pendentes"
                // com periodo=todos e do aviso da Home; não carrega os pedidos
                resultado = await context.Pedidos.CountAsync(p => p.Status == StatusPedido.Pendente);

            // Guarda também o "não é ADM" (null), para a segunda chamada não autorizar de novo
            HttpContext.Items[ChaveCache] = resultado;
            return resultado;
        }
    }
}
