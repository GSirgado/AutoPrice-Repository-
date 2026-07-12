using AutoMarket.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<AnuncioImg> AnuncioImagens { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }

        public DbSet<Favorito> Favoritos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Mensagem>(entity =>
            {
                entity.HasOne(m => m.Remetente)
                      .WithMany()
                      .HasForeignKey(m => m.RemetenteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Destinatario)
                      .WithMany()
                      .HasForeignKey(m => m.DestinatarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Anuncio)
                      .WithMany()
                      .HasForeignKey(m => m.AnuncioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Favorito>(entity =>
            {
                entity.HasKey(f => new { f.UtilizadorId, f.AnuncioId });

                entity.HasOne(f => f.Utilizador)
                      .WithMany()
                      .HasForeignKey(f => f.UtilizadorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Anuncio)
                      .WithMany()
                      .HasForeignKey(f => f.AnuncioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}