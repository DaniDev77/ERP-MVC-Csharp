using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
    public class Usuario
    {
        [Display(Name = "Código")]
        public int UsuarioId { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "O Email é obrigatório.")]
        public string Email { get; set; }

        [Display(Name = "Telefone")]
        [Required(ErrorMessage = "O Telefone é obrigatório.")]
        public string Phone { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O CPF é obrigatório.")] 
        public string CPF { get; set; }

        // Nulo = conta criada pelo próprio usuário, aguardando um ADM dar a função (vendedor ou padrão).
        // Sem função, a conta não acessa venda, compra nem desempenho.
        [Display(Name = "Função")]
        public int? FuncaoId { get; set; }
        public Funcao? Funcao { get; set; }

        // Campo legado: senhas em texto puro guardadas aqui. A senha de acesso é do Identity (com hash),
        // então este campo não é mais preenchido nem exibido. Para remover os valores antigos do banco,
        // rode: UPDATE Usuario SET Password = ''
        [Display(Name = "Senha")]
        public string? Password { get; set; } = string.Empty;

        // Relacionamento com o IdentityUser
        public Guid? AppUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }
    }
}
