using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    public class Funcao
    {
        [Display(Name = "Código")]
        public int FuncaoId { get; set; }

        [Display(Name = "Nome de Função no Sistema")]
        [Required(ErrorMessage = "O Nome de Função no Sistema é obrigatório.")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        public string Descricao { get; set; } 

    }
}
