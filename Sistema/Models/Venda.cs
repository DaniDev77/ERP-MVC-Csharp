using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    // Novo: venda registrada direto por um vendedor ou ADM, sem passar por pedido.
    // Antes ficava na tabela Pedido com Tipo = Venda
    public class Venda
    {
        public int VendaId { get; set; }

        public DateTime DataHora { get; set; }

        public decimal Total { get; set; }

        // Usuário padrão que recebe o produto
        public int CompradorId { get; set; }
        public Usuario? Comprador { get; set; }

        // Vendedor (ou ADM) que registrou a venda; sempre preenchido
        public int VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        // Localização do comprador no momento da venda (cópia do perfil dele; nula se não informou)
        [StringLength(2)]
        public string? Uf { get; set; }
        [StringLength(100)]
        public string? Cidade { get; set; }
        public int? CodigoMunicipio { get; set; }

        public List<VendaItem> Itens { get; set; } = new();
    }

    // Produto de uma venda, com o preço gravado no momento da venda
    public class VendaItem
    {
        public int VendaItemId { get; set; }

        public int VendaId { get; set; }
        public Venda? Venda { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }
    }
}
