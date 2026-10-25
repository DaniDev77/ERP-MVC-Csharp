using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Models;

namespace Sistema.Data
{
 
        public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
        {
        public DbSet<Funcao> Funcoes { get; set; }
        public DbSet<Fonecedor> Fornecedores { get; set; }
        public DbSet<TipoProduto> TiposProdutos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        // Produtos, compras/vendas e os itens de cada pedido
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Funcao>().ToTable("Funcao");
            builder.Entity<Fonecedor>().ToTable("Fornecedor");
            builder.Entity<TipoProduto>().ToTable("TipoProduto");
            builder.Entity<Usuario>().ToTable("Usuario");
            // Apagar uma função não apaga os usuários dela (antes a exclusão era em cascata)
            builder.Entity<Usuario>().HasOne(u => u.Funcao).WithMany().HasForeignKey(u => u.FuncaoId).OnDelete(DeleteBehavior.Restrict);
            // Uma conta de login liga-se a no máximo um cadastro de usuário
            builder.Entity<Usuario>().HasIndex(u => u.AppUserId).IsUnique().HasFilter("[AppUserId] IS NOT NULL");
            // Configurações adicionais do modelo podem ser feitas aqui

            // Produto: liga com Fornecedor e TipoProduto; Restrict impede apagar um fornecedor/tipo que ainda tenha produto
            builder.Entity<Produto>().ToTable("Produto");
            builder.Entity<Produto>().Property(p => p.Preco).HasPrecision(18, 2);
            builder.Entity<Produto>().HasOne(p => p.Fornecedor).WithMany().HasForeignKey(p => p.FornecedorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Produto>().HasOne(p => p.TipoProduto).WithMany().HasForeignKey(p => p.TipoProdutoId).OnDelete(DeleteBehavior.Restrict);

            // Pedido: comprador e vendedor são usuários; Restrict impede apagar um usuário que já tenha pedidos
            builder.Entity<Pedido>().ToTable("Pedido");
            builder.Entity<Pedido>().Property(p => p.Total).HasPrecision(18, 2);
            builder.Entity<Pedido>().HasOne(p => p.Comprador).WithMany().HasForeignKey(p => p.CompradorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Pedido>().HasOne(p => p.Vendedor).WithMany().HasForeignKey(p => p.VendedorId).OnDelete(DeleteBehavior.Restrict);

            // Itens somem junto com o pedido; produto de item não pode ser apagado
            builder.Entity<PedidoItem>().ToTable("PedidoItem");
            builder.Entity<PedidoItem>().Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            builder.Entity<PedidoItem>().HasOne(i => i.Pedido).WithMany(p => p.Itens).HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PedidoItem>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        }
    }

        
 }
