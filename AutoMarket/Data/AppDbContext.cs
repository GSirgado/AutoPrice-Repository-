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
    }
}