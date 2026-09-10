using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    public class Fonecedor
    {
      [Display(Name = "Código")]
      public Guid FonecedorId { get; set; }

      [Display(Name = "Nome de Fornecedor")]
      [Required(ErrorMessage = "O Nome de Fornecedor é obrigatório.")]
      public string FonecedorNome { get; set; }
      
      [Display(Name = "Descrição")]
      [Required(ErrorMessage = "A Descrição é obrigatória.")]
      public string FonecedorDescricao { get; set; }

      [Display(Name = "CNPJ")]
      [Required(ErrorMessage = "O CNPJ é obrigatório.")]
      public string CNPJ { get; set; }

        [Display(Name = "Telefone")]
        [Required(ErrorMessage = "O Telefone  é obrigatório.")]
        public string FonecedorTelefone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "O Email é obrigatório.")]
        public string FonecedorEmail { get; set; }

    }
}
