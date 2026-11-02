using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    // Novo: compra confirmada. Só existe depois que o ADM confirma um Pedido do cliente
    // (as compras antigas, de antes da separação, vieram da tabela Pedido sem PedidoId)
    public class Compra
    {
        public int CompraId { get; set; }

        // Momento da confirmação (nas compras antigas, o momento em que foram registradas)
        public DateTime DataHora { get; set; }

        public decimal Total { get; set; }

        public int CompradorId { get; set; }
        public Usuario? Comprador { get; set; }

        // Pedido que originou a compra; nulo nas compras anteriores ao fluxo de pedidos
        public int? PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        // Localização do cliente copiada do pedido (usada no painel de compras por localidade)
        [StringLength(2)]
        public string? Uf { get; set; }
        [StringLength(100)]
        public string? Cidade { get; set; }
        public int? CodigoMunicipio { get; set; }

        public List<CompraItem> Itens { get; set; } = new();
    }

    // Produto de uma compra, com o preço do pedido
    public class CompraItem
    {
        public int CompraItemId { get; set; }

        public int CompraId { get; set; }
        public Compra? Compra { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }
    }
}
