using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;

namespace Sistema.Data
{
 
        public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
        {
        public DbSet<Funcao> Funcoes { get; set; }
        public DbSet<Fonecedor> Fonecedores { get; set; }
        public DbSet<TipoProduto> TiposProdutos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Funcao>().ToTable("Funcao");
            builder.Entity<Fonecedor>().ToTable("Fonecedor");
            builder.Entity<TipoProduto>().ToTable("TipoProduto");
            builder.Entity<Usuario>().ToTable("Usuario");
            // Configurações adicionais do modelo podem ser feitas aqui
        }
    }

        
 }
