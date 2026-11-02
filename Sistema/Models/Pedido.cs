using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    // Alterado: o enum TipoPedido saiu. Pedido agora é só a solicitação do cliente (usuário padrão),
    // que espera a confirmação da loja. Compra e Venda ganharam tabelas próprias (Compra.cs e Venda.cs).
    public enum StatusPedido
    {
        // Enviado pelo cliente, aguardando o ADM
        Pendente = 0,
        // Aceito pelo ADM: o estoque foi baixado e a Compra foi criada
        Confirmado = 1,
        // Negado pelo ADM (com motivo opcional)
        Recusado = 2,
        // Desistência do próprio cliente enquanto estava pendente
        Cancelado = 3
    }

    // Pedido do cliente. Só vira Compra depois de confirmado; enquanto pendente, não mexe no estoque
    public class Pedido
    {
        public int PedidoId { get; set; }

        // Data e hora em que o cliente enviou o pedido
        public DateTime DataHora { get; set; }

        public StatusPedido Status { get; set; } = StatusPedido.Pendente;

        // Soma dos itens com o preço do momento do pedido (é o valor que o cliente viu)
        public decimal Total { get; set; }

        // Cliente que fez o pedido (usuário padrão ou ADM comprando para si)
        public int CompradorId { get; set; }
        public Usuario? Comprador { get; set; }

        // Novo: localização do cliente no momento do pedido (cópia do perfil; nula se ele não informou)
        [StringLength(2)]
        public string? Uf { get; set; }
        [StringLength(100)]
        public string? Cidade { get; set; }
        public int? CodigoMunicipio { get; set; }

        // Novo: quando e por quem o pedido foi confirmado, recusado ou cancelado
        public DateTime? DataDecisao { get; set; }
        public int? DecididoPorId { get; set; }
        public Usuario? DecididoPor { get; set; }

        // Novo: motivo informado pelo ADM ao recusar
        [StringLength(300)]
        public string? Motivo { get; set; }

        // Novo: a compra gerada na confirmação (nula enquanto não confirmado)
        public Compra? Compra { get; set; }

        public List<PedidoItem> Itens { get; set; } = new();

        // Novo: controle de concorrência. Dois ADMs confirmando, ou o ADM confirmando enquanto o cliente cancela,
        // não podem decidir o mesmo pedido duas vezes
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }

    // Produto de um pedido, com o preço gravado no momento do pedido
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
