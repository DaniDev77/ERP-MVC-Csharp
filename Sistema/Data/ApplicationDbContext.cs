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
        public DbSet<Produto> Produtos { get; set; }
        // Alterado: Pedido é só o pedido do cliente (aguardando confirmação); compras e vendas ganharam tabelas próprias
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItens { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraItem> CompraItens { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<VendaItem> VendaItens { get; set; }

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

            // Alterado: Pedido perdeu Tipo e VendedorId (venda não passa mais por pedido) e ganhou status,
            // decisão do ADM e localização. Restrict impede apagar um usuário que já tenha pedidos
            builder.Entity<Pedido>().ToTable("Pedido");
            builder.Entity<Pedido>().Property(p => p.Total).HasPrecision(18, 2);
            builder.Entity<Pedido>().HasOne(p => p.Comprador).WithMany().HasForeignKey(p => p.CompradorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Pedido>().HasOne(p => p.DecididoPor).WithMany().HasForeignKey(p => p.DecididoPorId).OnDelete(DeleteBehavior.Restrict);
            // A tela de pedidos do ADM filtra por status e ordena por data
            builder.Entity<Pedido>().HasIndex(p => new { p.Status, p.DataHora });

            // Itens somem junto com o pedido; produto de item não pode ser apagado
            builder.Entity<PedidoItem>().ToTable("PedidoItem");
            builder.Entity<PedidoItem>().Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            builder.Entity<PedidoItem>().HasOne(i => i.Pedido).WithMany(p => p.Itens).HasForeignKey(i => i.PedidoId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PedidoItem>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);

            // Novo: Compra (só existe após a confirmação do pedido). Um pedido gera no máximo uma compra
            builder.Entity<Compra>().ToTable("Compra");
            builder.Entity<Compra>().Property(c => c.Total).HasPrecision(18, 2);
            builder.Entity<Compra>().HasOne(c => c.Comprador).WithMany().HasForeignKey(c => c.CompradorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Compra>().HasOne(c => c.Pedido).WithOne(p => p.Compra).HasForeignKey<Compra>(c => c.PedidoId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Compra>().HasIndex(c => c.PedidoId).IsUnique().HasFilter("[PedidoId] IS NOT NULL");
            builder.Entity<Compra>().HasIndex(c => c.DataHora);

            builder.Entity<CompraItem>().ToTable("CompraItem");
            builder.Entity<CompraItem>().Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            builder.Entity<CompraItem>().HasOne(i => i.Compra).WithMany(c => c.Itens).HasForeignKey(i => i.CompraId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CompraItem>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);

            // Novo: Venda registrada direto pelo vendedor/ADM
            builder.Entity<Venda>().ToTable("Venda");
            builder.Entity<Venda>().Property(v => v.Total).HasPrecision(18, 2);
            builder.Entity<Venda>().HasOne(v => v.Comprador).WithMany().HasForeignKey(v => v.CompradorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Venda>().HasOne(v => v.Vendedor).WithMany().HasForeignKey(v => v.VendedorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Venda>().HasIndex(v => v.DataHora);

            builder.Entity<VendaItem>().ToTable("VendaItem");
            builder.Entity<VendaItem>().Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            builder.Entity<VendaItem>().HasOne(i => i.Venda).WithMany(v => v.Itens).HasForeignKey(i => i.VendaId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<VendaItem>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        }
    }

        
 }
