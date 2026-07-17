using AutoMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages.FavoritosPages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<FavoritoAnuncioDto> Favoritos { get; set; } = new();

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Mesma consulta que antes vivia em GET /api/favoritos, agora feita
            // diretamente aqui.
            Favoritos = await _db.Favoritos
                .Where(f => f.UtilizadorId == userId && f.Anuncio != null)
                .Include(f => f.Anuncio!).ThenInclude(a => a.Imagens)
                .OrderByDescending(f => f.CriadoEm)
                .Select(f => new FavoritoAnuncioDto
                {
                    Id = f.Anuncio!.Id,
                    Titulo = f.Anuncio.Titulo,
                    Marca = f.Anuncio.Marca,
                    Modelo = f.Anuncio.Modelo,
                    Tipo = f.Anuncio.Tipo,
                    Ano = f.Anuncio.Ano,
                    Preco = f.Anuncio.Preco,
                    Kilometragem = f.Anuncio.Kilometragem,
                    Combustivel = f.Anuncio.Combustivel,
                    Condicao = f.Anuncio.Condicao,
                    Imagens = f.Anuncio.Imagens.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Page();
        }

        // Substitui o antigo fetch("DELETE /api/favoritos/{id}") feito em JS.
        public async Task<IActionResult> OnPostRemoverAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var favorito = await _db.Favoritos
                .FirstOrDefaultAsync(f => f.UtilizadorId == userId && f.AnuncioId == id);

            if (favorito != null)
            {
                _db.Favoritos.Remove(favorito);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }

    public class FavoritoAnuncioDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public List<string> Imagens { get; set; } = new();
    }
}
