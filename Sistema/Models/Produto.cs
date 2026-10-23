using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    public class Produto
    {
        // Quantidade igual ou abaixo deste valor gera notificação de estoque baixo para o ADM
        public const int EstoqueMinimo = 10;

        [Display(Name = "Código")]
        public int ProdutoId { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Preço")]
        [Range(0.01, 999999999, ErrorMessage = "O Preço deve ser maior que zero.")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Display(Name = "Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O Estoque não pode ser negativo.")]
        public int Estoque { get; set; }

        [Display(Name = "Fornecedor")]
        [Required(ErrorMessage = "O Fornecedor é obrigatório.")]
        public Guid FornecedorId { get; set; }
        public Fonecedor? Fornecedor { get; set; }

        [Display(Name = "Tipo de Produto")]
        [Required(ErrorMessage = "O Tipo de Produto é obrigatório.")]
        public int TipoProdutoId { get; set; }
        public TipoProduto? TipoProduto { get; set; }
    }
}
