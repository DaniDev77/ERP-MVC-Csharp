using Microsoft.AspNetCore.Identity;

namespace Sistema.Models
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FuncaoNome { get; set; }
        public Funcao? Funcao { get; set; }
        public string? Password { get; set; } = string.Empty;
        
        // Relacionamento com o IdentityUser
        public Guid? AppUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }
    }
}
