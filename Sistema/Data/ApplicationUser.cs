using Microsoft.AspNetCore.Identity;
namespace Sistema.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; }
    public DateTime DataNascimento { get; set; }
}
