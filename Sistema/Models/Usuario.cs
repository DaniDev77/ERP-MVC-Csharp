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

        [Display(Name = "Função")]
        [Required(ErrorMessage = "A Função é obrigatória.")]
        public int FuncaoId { get; set; }
        public Funcao? Funcao { get; set; }

        [Display(Name = "Senha")]
        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string? Password { get; set; } = string.Empty;

        // Relacionamento com o IdentityUser
        [Display(Name = "Id de Usuário")]
        [Required(ErrorMessage = "O Id de Usuário é obrigatório.")]
        public Guid? AppUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }
    }
}
