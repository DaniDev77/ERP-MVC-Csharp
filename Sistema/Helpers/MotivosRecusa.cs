using Sistema.Models;

namespace Sistema.Helpers
{
    // Novo (direção D, B3, decisão do dono): atalhos que preenchem o motivo da recusa. Editar aqui muda a tela.
    // O campo continua livre; o servidor só normaliza e limita a 300 caracteres (HF-006) e não valida contra esta lista
    public static class MotivosRecusa
    {
        public static readonly IReadOnlyList<(string Rotulo, string Texto)> Padrao =
        [
            ("Sem estoque", "Sem estoque de um dos itens no momento."),
            ("Preço mudou", "O preço de um item mudou. Faça o pedido de novo para ver o valor atual."),
            ("Pedido repetido", "Pedido repetido: o mesmo pedido chegou duas vezes.")
        ];

        // "Sem estoque" cita o primeiro produto em falta (EstoqueCurto da B2), quando houver
        public static IEnumerable<(string Rotulo, string Texto)> Para(Pedido pedido, IReadOnlyDictionary<int, int> estoqueCurto)
        {
            var falta = pedido.Itens.FirstOrDefault(i => estoqueCurto.ContainsKey(i.PedidoItemId))?.Produto?.Name;
            foreach (var m in Padrao)
                yield return m.Rotulo == "Sem estoque" && falta != null ? (m.Rotulo, $"Sem estoque de {falta} no momento.") : m;
        }
    }
}
