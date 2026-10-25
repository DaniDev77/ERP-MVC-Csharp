namespace Sistema.Helpers
{
    // Ícones de traço (16px, contorno) para a navegação e o perfil.
    // Os desenhos são constantes do código, então podem ser colocados na página sem escape.
    public static class Icones
    {
        // Moldura comum: 24x24, traço que acompanha a cor do texto (currentColor)
        private const string Moldura = "<svg class=\"nav-icon\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{0}</svg>";

        private static readonly Dictionary<string, string> Desenhos = new()
        {
            ["inicio"] = "<path d=\"M3 10.5 12 3l9 7.5\"/><path d=\"M5 9.5V21h14V9.5\"/>",
            ["venda"] = "<path d=\"M3 12V4h8l10 10-8 8L3 12z\"/><circle cx=\"7.5\" cy=\"8.5\" r=\"1.5\"/>",
            ["compra"] = "<circle cx=\"9\" cy=\"20\" r=\"1.5\"/><circle cx=\"18\" cy=\"20\" r=\"1.5\"/><path d=\"M3 4h2.5l2.4 11h10.6l2-7H7\"/>",
            ["desempenho"] = "<path d=\"M4 20V11\"/><path d=\"M10 20V5\"/><path d=\"M16 20v-6\"/><path d=\"M3 20h18\"/>",
            ["produtos"] = "<path d=\"M21 8 12 3 3 8v8l9 5 9-5V8z\"/><path d=\"M3 8l9 5 9-5M12 13v8\"/>",
            ["fornecedores"] = "<path d=\"M2 6h12v10H2z\"/><path d=\"M14 10h4l4 4v2h-8\"/><circle cx=\"6\" cy=\"18\" r=\"2\"/><circle cx=\"18\" cy=\"18\" r=\"2\"/>",
            ["tipos"] = "<path d=\"M12 3 3 8l9 5 9-5-9-5z\"/><path d=\"M3 13l9 5 9-5\"/>",
            ["usuarios"] = "<circle cx=\"9\" cy=\"8\" r=\"3.5\"/><path d=\"M2.5 20c.8-3.4 3.4-5.5 6.5-5.5s5.7 2.1 6.5 5.5\"/><path d=\"M16 4.5a3.5 3.5 0 0 1 0 7\"/>",
            ["funcoes"] = "<path d=\"M12 3 4 6v6c0 4.5 3.4 8 8 9 4.6-1 8-4.5 8-9V6l-8-3z\"/><path d=\"m9 12 2 2 4-4\"/>",
            ["perfil"] = "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21c1.2-4 4.2-6 8-6s6.8 2 8 6\"/>",
            ["sair"] = "<path d=\"M15 4h4v16h-4\"/><path d=\"M10 12H3M6 9l-3 3 3 3\"/>",
            ["check"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"m8 12.5 2.5 2.5L16 9.5\"/>",
            ["alerta"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7.5v5.5M12 16.5v.01\"/>",
            ["buscar"] = "<circle cx=\"11\" cy=\"11\" r=\"6.5\"/><path d=\"m20 20-4.2-4.2\"/>",
            ["mais"] = "<path d=\"M12 5v14M5 12h14\"/>",
            ["espera"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/>",
            ["seta"] = "<path d=\"M5 12h14M13 6l6 6-6 6\"/>",
        };

        // Devolve o SVG do ícone pelo nome; nome desconhecido não gera nada
        public static string Get(string nome)
        {
            return Desenhos.TryGetValue(nome, out var desenho) ? string.Format(Moldura, desenho) : string.Empty;
        }
    }
}
