namespace Sistema.Models
{
    // Números do painel inicial: o ADM vê a loja toda; vendedor e padrão veem só o próprio mês
    public class InicioViewModel
    {
        public bool Logado { get; set; }

        public bool EhAdmin { get; set; }

        public bool EhVendedor { get; set; }

        public bool EhPadrao { get; set; }

        // Conta sem função ainda (aguardando o ADM liberar)
        public bool Pendente { get; set; }

        public string NomeUsuario { get; set; } = string.Empty;

        // Primeiro dia do mês exibido
        public DateTime Mes { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // Totais do mês (ADM: a loja; vendedor: as próprias vendas; padrão: as próprias compras)
        public decimal ValorVendasMes { get; set; }

        public int QtdVendasMes { get; set; }

        public decimal ValorComprasMes { get; set; }

        public int QtdComprasMes { get; set; }

        // Só ADM: estoque
        public int TotalEstoqueBaixo { get; set; }

        public decimal ValorEmEstoque { get; set; }

        // Só ADM: quantos produtos existem e quantos estão zerados (o resumo no pé do cartão de reposição)
        public int TotalProdutos { get; set; }

        public int TotalSemEstoque { get; set; }

        public List<Produto> EstoqueBaixo { get; set; } = new();

        public List<Pedido> UltimosPedidos { get; set; } = new();

        // Gráfico de vendas por dia do mês, até hoje: pontos já prontos para o <polyline> do SVG
        public string PontosVendasPorDia { get; set; } = string.Empty;

        public decimal MaiorVendaDoDia { get; set; }

        // Vendas somadas de cada dia do mês, do dia 1 até hoje (o gráfico de colunas do início)
        public decimal[] VendasPorDia { get; set; } = Array.Empty<decimal>();
    }
}
