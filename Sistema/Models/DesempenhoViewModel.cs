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

        // Vendas agrupadas por vendedor no período (só o ADM vê esta tabela)
        public List<LinhaVendedor> PorVendedor { get; set; } = new();
    }

    // Uma linha da tabela "vendas por vendedor"
    public class LinhaVendedor
    {
        public string Nome { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal Valor { get; set; }
    }
}
