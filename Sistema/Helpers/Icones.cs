namespace Sistema.Helpers
{
    // Ícones de linha (contorno) usados em todo o sistema: menu, botões, campos e cards.
    // Os desenhos são constantes do código, então podem ser colocados na página sem escape.
    public static class Icones
    {
        // Moldura comum: 24x24, traço que acompanha a cor do texto (currentColor); o tamanho final vem do CSS
        private const string Moldura = "<svg class=\"nav-icon\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\" focusable=\"false\">{0}</svg>";

        private static readonly Dictionary<string, string> Desenhos = new()
        {
            ["inicio"] = "<path d=\"M3 10.5 12 3l9 7.5\"/><path d=\"M5 9.5V21h14V9.5\"/><path d=\"M10 21v-6h4v6\"/>",
            ["venda"] = "<path d=\"M3 12V4h8l10 10-8 8L3 12z\"/><circle cx=\"7.5\" cy=\"8.5\" r=\"1.5\"/>",
            ["compra"] = "<circle cx=\"9\" cy=\"20\" r=\"1.5\"/><circle cx=\"18\" cy=\"20\" r=\"1.5\"/><path d=\"M3 4h2.5l2.4 11h10.6l2-7H7\"/>",
            ["desempenho"] = "<path d=\"M4 20V11\"/><path d=\"M10 20V5\"/><path d=\"M16 20v-6\"/><path d=\"M3 20h18\"/>",
            ["produtos"] = "<path d=\"M21 8 12 3 3 8v8l9 5 9-5V8z\"/><path d=\"M3 8l9 5 9-5M12 13v8\"/>",
            ["fornecedores"] = "<path d=\"M2 6h12v10H2z\"/><path d=\"M14 10h4l4 4v2h-8\"/><circle cx=\"6\" cy=\"18\" r=\"2\"/><circle cx=\"18\" cy=\"18\" r=\"2\"/>",
            ["tipos"] = "<path d=\"M12 3 3 8l9 5 9-5-9-5z\"/><path d=\"M3 13l9 5 9-5\"/>",
            ["usuarios"] = "<circle cx=\"9\" cy=\"8\" r=\"3.5\"/><path d=\"M2.5 20c.8-3.4 3.4-5.5 6.5-5.5s5.7 2.1 6.5 5.5\"/><path d=\"M16 4.5a3.5 3.5 0 0 1 0 7\"/><path d=\"M18 14.8c1.8.7 3 2.5 3.5 5.2\"/>",
            ["funcoes"] = "<path d=\"M12 3 4 6v6c0 4.5 3.4 8 8 9 4.6-1 8-4.5 8-9V6l-8-3z\"/><path d=\"m9 12 2 2 4-4\"/>",
            ["perfil"] = "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21c1.2-4 4.2-6 8-6s6.8 2 8 6\"/>",
            ["sair"] = "<path d=\"M15 4h4v16h-4\"/><path d=\"M10 12H3M6 9l-3 3 3 3\"/>",
            ["check"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"m8 12.5 2.5 2.5L16 9.5\"/>",
            ["alerta"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7.5v5.5M12 16.5v.01\"/>",
            ["buscar"] = "<circle cx=\"11\" cy=\"11\" r=\"6.5\"/><path d=\"m20 20-4.2-4.2\"/>",
            ["mais"] = "<path d=\"M12 5v14M5 12h14\"/>",
            ["espera"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/>",
            ["seta"] = "<path d=\"M5 12h14M13 6l6 6-6 6\"/>",
            // Ícones novos do redesign (ações, campos, tema e status)
            ["editar"] = "<path d=\"M4 20h4L19 9a2.8 2.8 0 0 0-4-4L4 16v4z\"/><path d=\"m13.5 6.5 4 4\"/>",
            ["excluir"] = "<path d=\"M4 7h16\"/><path d=\"M10 11v6M14 11v6\"/><path d=\"M5.5 7l1 12.5A2 2 0 0 0 8.5 21h7a2 2 0 0 0 2-1.5L18.5 7\"/><path d=\"M9 7V4.5h6V7\"/>",
            ["olho"] = "<path d=\"M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z\"/><circle cx=\"12\" cy=\"12\" r=\"3\"/>",
            ["olho-off"] = "<path d=\"M3 3l18 18\"/><path d=\"M10.6 5.1A10 10 0 0 1 12 5c6.4 0 10 7 10 7a17 17 0 0 1-3.2 4.1M6.6 6.6C3.8 8.4 2 12 2 12s3.6 7 10 7c1.9 0 3.5-.6 4.9-1.4\"/><path d=\"M9.9 9.9a3 3 0 0 0 4.2 4.2\"/>",
            ["lua"] = "<path d=\"M20 14.5A8 8 0 1 1 9.5 4a6.5 6.5 0 0 0 10.5 10.5z\"/>",
            ["sol"] = "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4\"/>",
            ["carrinho"] = "<circle cx=\"9\" cy=\"20\" r=\"1.5\"/><circle cx=\"18\" cy=\"20\" r=\"1.5\"/><path d=\"M3 4h2.5l2.4 11h10.6l2-7H7\"/><path d=\"M12 8v4M10 10h4\"/>",
            ["email"] = "<rect x=\"3\" y=\"5\" width=\"18\" height=\"14\" rx=\"2.5\"/><path d=\"m4 7 8 6 8-6\"/>",
            ["telefone"] = "<path d=\"M5 4h3l1.5 4.5-2 1.5a11 11 0 0 0 6.5 6.5l1.5-2L20 16v3a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2z\"/>",
            ["cadeado"] = "<rect x=\"4.5\" y=\"10.5\" width=\"15\" height=\"10\" rx=\"2.5\"/><path d=\"M8 10.5V7.5a4 4 0 0 1 8 0v3\"/><path d=\"M12 15v2\"/>",
            ["menu"] = "<path d=\"M4 7h16M4 12h16M4 17h10\"/>",
            ["fechar"] = "<path d=\"M6 6l12 12M18 6 6 18\"/>",
            ["pontos"] = "<circle cx=\"12\" cy=\"5.5\" r=\"1.2\"/><circle cx=\"12\" cy=\"12\" r=\"1.2\"/><circle cx=\"12\" cy=\"18.5\" r=\"1.2\"/>",
            ["chevron"] = "<path d=\"m6 9 6 6 6-6\"/>",
            ["voltar"] = "<path d=\"M19 12H5M11 6l-6 6 6 6\"/>",
            ["menos"] = "<path d=\"M5 12h14\"/>",
            ["moeda"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M15 9.2c-.6-.9-1.7-1.4-3-1.4-1.7 0-3 .9-3 2.1 0 2.9 6 1.5 6 4.3 0 1.2-1.3 2.1-3 2.1-1.4 0-2.5-.6-3.1-1.5\"/><path d=\"M12 6v1.8M12 16.3V18\"/>",
            ["tendencia"] = "<path d=\"M3 17l6-6 4 4 8-8\"/><path d=\"M15 7h6v6\"/>",
            ["calendario"] = "<rect x=\"3.5\" y=\"5\" width=\"17\" height=\"15.5\" rx=\"2.5\"/><path d=\"M3.5 10h17M8 3v4M16 3v4\"/>",
            ["documento"] = "<path d=\"M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8l-5-5z\"/><path d=\"M14 3v5h5M9 13h6M9 17h4\"/>",
            ["predio"] = "<path d=\"M4 21V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v16\"/><path d=\"M16 9h2a2 2 0 0 1 2 2v10M3 21h18M8 7h4M8 11h4M8 15h4\"/>",
            ["etiqueta"] = "<path d=\"M20.6 13.4 13.4 20.6a2 2 0 0 1-2.8 0L3 13V3h10l7.6 7.6a2 2 0 0 1 0 2.8z\"/><circle cx=\"7.5\" cy=\"7.5\" r=\"1.3\"/>",
            ["texto"] = "<path d=\"M4 6h16M4 12h16M4 18h10\"/>",
            ["id"] = "<rect x=\"3\" y=\"5\" width=\"18\" height=\"14\" rx=\"2.5\"/><circle cx=\"9\" cy=\"11\" r=\"2\"/><path d=\"M6 16c.6-1.4 1.7-2 3-2s2.4.6 3 2M15 10h3M15 13h3\"/>",
            ["caixa"] = "<path d=\"M3 7.5 12 3l9 4.5v9L12 21l-9-4.5v-9z\"/><path d=\"M3 7.5 12 12l9-4.5M12 12v9M7.5 5.2l9 4.6\"/>",
            ["sino"] = "<path d=\"M6 16V11a6 6 0 1 1 12 0v5l1.5 2h-15L6 16z\"/><path d=\"M10 20.5a2 2 0 0 0 4 0\"/>",
            ["info"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v5.5M12 7.5v.01\"/>",
            ["brilho"] = "<path d=\"M12 3v4M12 17v4M3 12h4M17 12h4\"/><path d=\"m12 8 1.2 2.8L16 12l-2.8 1.2L12 16l-1.2-2.8L8 12l2.8-1.2z\"/>",
            ["ok"] = "<path d=\"m5 12.5 4.5 4.5L19 7.5\"/>",
            ["chave"] = "<circle cx=\"8\" cy=\"15\" r=\"4\"/><path d=\"m11 12 9-9M17 6l3 3M14.5 8.5l2 2\"/>",
            ["link"] = "<path d=\"M10 14a4 4 0 0 0 5.7 0l3-3a4 4 0 0 0-5.7-5.7l-1 1\"/><path d=\"M14 10a4 4 0 0 0-5.7 0l-3 3a4 4 0 0 0 5.7 5.7l1-1\"/>",
            ["recibo"] = "<path d=\"M6 3h12v18l-3-2-3 2-3-2-3 2V3z\"/><path d=\"M9 8h6M9 12h6M9 16h3\"/>",
            ["filtro"] = "<path d=\"M4 5h16l-6 7.5V19l-4 2v-8.5L4 5z\"/>",
            ["externo"] = "<path d=\"M14 4h6v6M20 4l-9 9\"/><path d=\"M18 14v4a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4\"/>",
            ["casa-erro"] = "<path d=\"M12 3 2.5 19.5h19L12 3z\"/><path d=\"M12 10v4.5M12 17.5v.01\"/>",
        };

        // Devolve o SVG do ícone pelo nome; nome desconhecido não gera nada
        public static string Get(string nome)
        {
            return Desenhos.TryGetValue(nome, out var desenho) ? string.Format(Moldura, desenho) : string.Empty;
        }
    }
}
