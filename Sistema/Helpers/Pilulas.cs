using System.Net;
using Sistema.Authorization;

namespace Sistema.Helpers
{
    // Selos de função usados nas listas. Tons pálidos com texto escuro; a cor fica só no que precisa de atenção.
    public static class Pilulas
    {
        // Selo da função do usuário. Sem função (conta aguardando liberação) aparece em âmbar, para o ADM ver de longe
        public static string Funcao(int? funcaoId, string? nome)
        {
            if (funcaoId == null)
                return Selo("pill-alerta", "Aguardando liberação");

            var classe = funcaoId switch
            {
                Politicas.FuncaoAdminId => "pill-forte",
                Politicas.FuncaoVendedorId => "pill-azul",
                _ => "pill"
            };
            // O nome vem do banco em caixa alta ("USUARIO PADRÃO"); no selo fica em caixa normal, exceto siglas curtas como ADM
            var texto = nome ?? "Sem nome";
            if (texto.Length > 3)
                texto = System.Globalization.CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(texto.ToLowerInvariant());
            return Selo(classe, texto);
        }

        // Selo do tipo de operação: venda (azul) ou compra (neutro)
        public static string Tipo(bool ehVenda)
        {
            return ehVenda ? Selo("pill-azul", "Venda") : Selo("pill", "Compra");
        }

        // Escapa o texto: nome vindo do banco nunca vira HTML executável
        private static string Selo(string classe, string texto)
        {
            return $"<span class=\"pill {classe}\">{WebUtility.HtmlEncode(texto)}</span>";
        }
    }
}
