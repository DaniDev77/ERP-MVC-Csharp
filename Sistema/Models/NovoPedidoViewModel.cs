namespace Sistema.Models
{
    // Formulário usado tanto na venda quanto na compra
    public class NovoPedidoViewModel
    {
        // Só usado na venda: CPF ou código do usuário padrão que vai receber o produto
        public string? Comprador { get; set; }

        // Linhas fixas do formulário; linhas sem produto são ignoradas
        public List<LinhaPedido> Linhas { get; set; } = new();
    }

    // Um produto escolhido com a quantidade desejada
    public class LinhaPedido
    {
        public int ProdutoId { get; set; }

        public int Quantidade { get; set; }
    }
}
