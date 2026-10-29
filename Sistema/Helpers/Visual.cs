namespace Sistema.Helpers
{
    // Ajudas de apresentação: iniciais para avatares/miniaturas e uma matiz estável por nome
    public static class Visual
    {
        // Matiz (0–359) calculada pelo nome: o mesmo produto/pessoa sempre tem a mesma cor.
        // Faixas sóbrias (azuis, azul-petróleo, verde-azulado, cinza-azulado); a saturação baixa fica no CSS (.avatar/.thumb)
        public static int Matiz(string? nome)
        {
            if (string.IsNullOrEmpty(nome)) return 214;
            var soma = 0;
            foreach (var c in nome) soma = unchecked(soma * 31 + c);
            // Sem violeta nem rosa: só matizes frias e contidas
            var faixas = new[] { 214, 205, 222, 198, 188, 178, 228, 210 };
            return faixas[Math.Abs(soma % faixas.Length)];
        }

        // Até duas iniciais: primeira letra do primeiro e do último nome
        public static string Iniciais(string? nome, int maximo = 2)
        {
            var partes = (nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1 || maximo == 1) return partes[0][..1].ToUpperInvariant();
            return (partes[0][..1] + partes[^1][..1]).ToUpperInvariant();
        }

        // Nome de função em caixa normal; siglas curtas (ADM) ficam como estão
        public static string NomeFuncao(string? nome)
        {
            if (string.IsNullOrEmpty(nome)) return "Sem função";
            return nome.Length > 3
                ? System.Globalization.CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(nome.ToLowerInvariant())
                : nome;
        }
    }
}
