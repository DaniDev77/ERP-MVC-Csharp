using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    public class TipoProduto
    {
        [Display(Name = "Código")]
        public int TipoProdutoId { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string TipoNome { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        public string TipoDescricao { get; set; }

    }
}
