namespace Sistema.Models
{
    // Compra: usuário padrão compra para si. Venda: vendedor registra a venda para um usuário padrão.
    public enum TipoPedido
    {
        Compra,
        Venda
    }

    // Cabeçalho de uma compra ou venda
    public class Pedido
    {
        public int PedidoId { get; set; }

        public TipoPedido Tipo { get; set; }

        public DateTime DataHora { get; set; }

        public decimal Total { get; set; }

        // Usuário que recebe o produto (sempre um usuário padrão)
        public int CompradorId { get; set; }
        public Usuario? Comprador { get; set; }

        // Vendedor que registrou a venda; nulo nas compras feitas pelo próprio usuário padrão
        public int? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        public List<PedidoItem> Itens { get; set; } = new();
    }

    // Produto de um pedido, com o preço gravado no momento da operação
    public class PedidoItem
    {
        public int PedidoItemId { get; set; }

        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }
    }
}
