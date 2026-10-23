namespace Sistema.Models
{
    // Totais de compras e vendas de um período (Inicio incluso, Fim excluso)
    public class DesempenhoViewModel
    {
        public string Periodo { get; set; } = "mes";

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        public int QtdCompras { get; set; }

        public decimal ValorCompras { get; set; }

        public int QtdVendas { get; set; }

        public decimal ValorVendas { get; set; }
    }
}
