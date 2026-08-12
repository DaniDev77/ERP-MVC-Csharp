namespace Sistema.Models
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Guid FuncaoId { get; set; }
        public Funcao? Funcao { get; set; }
        public string? Password { get; set; } = string.Empty;

    }
}
