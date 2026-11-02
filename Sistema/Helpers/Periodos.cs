namespace Sistema.Helpers
{
    // Novo: cálculo de período movido do DesempenhoController para cá, porque agora também é usado
    // na tela de pedidos e no painel de compras por localidade. As regras são as mesmas de antes.
    public static class Periodos
    {
        // Períodos aceitos nos filtros (valor da query string → texto do botão)
        public static readonly (string Valor, string Texto)[] Opcoes =
        {
            ("semana", "Semana"), ("mes", "Mês"), ("bimestre", "Bimestre"), ("semestre", "Semestre"), ("ano", "Ano")
        };

        // Converte o nome do período em data inicial; o fim é amanhã (exclusivo), ou seja, inclui hoje.
        // "todos" volta do ano 2000 até hoje (usado só na lista de pedidos)
        public static (DateTime Inicio, DateTime Fim) Calcular(string? periodo)
        {
            var hoje = DateTime.Today;

            var inicio = periodo switch
            {
                // Semana começa na segunda-feira
                "semana" => hoje.AddDays(-(((int)hoje.DayOfWeek + 6) % 7)),
                // Bimestres: jan-fev, mar-abr, mai-jun, jul-ago, set-out, nov-dez
                "bimestre" => new DateTime(hoje.Year, ((hoje.Month - 1) / 2) * 2 + 1, 1),
                // Semestres: jan-jun e jul-dez
                "semestre" => new DateTime(hoje.Year, hoje.Month <= 6 ? 1 : 7, 1),
                "ano" => new DateTime(hoje.Year, 1, 1),
                "todos" => new DateTime(2000, 1, 1),
                _ => new DateTime(hoje.Year, hoje.Month, 1)
            };

            return (inicio, hoje.AddDays(1));
        }

        // Normaliza o valor recebido na URL: qualquer coisa desconhecida vira "mes"
        public static string Normalizar(string? periodo, bool aceitaTodos = false)
        {
            if (aceitaTodos && periodo == "todos")
                return periodo;
            return Opcoes.Any(o => o.Valor == periodo) ? periodo! : "mes";
        }
    }
}
