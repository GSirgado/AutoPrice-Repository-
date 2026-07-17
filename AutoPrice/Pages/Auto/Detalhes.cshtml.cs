using AutoMarket.Data;
using AutoMarket.Models;
using AutoPrice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages.AutoPages
{
    public class DetalhesModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnuncioView? Anuncio { get; set; }

        public DetalhesModel(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null)
                return NotFound();

            string? nomeVendedor = null;
            if (!string.IsNullOrEmpty(anuncio.VendedorId))
            {
                var vendedor = await _userManager.FindByIdAsync(anuncio.VendedorId);
                nomeVendedor = vendedor?.NomeCompleto;
            }

            Anuncio = new AnuncioView
            {
                Id = anuncio.Id,
                Titulo = anuncio.Titulo,
                Descricao = anuncio.Descricao,
                Marca = anuncio.Marca,
                Modelo = anuncio.Modelo,
                Tipo = anuncio.Tipo,
                Ano = anuncio.Ano,
                Preco = anuncio.Preco,
                Kilometragem = anuncio.Kilometragem,
                Combustivel = anuncio.Combustivel,
                Condicao = anuncio.Condicao,
                Cor = anuncio.Cor,
                Transmissao = anuncio.Transmissao,
                Potencia = anuncio.Potencia,
                CategoriaId = anuncio.CategoriaId,
                CategoriaNome = anuncio.Categoria?.Nome,
                VendedorId = anuncio.VendedorId,
                VendedorNome = nomeVendedor,
                Imagens = anuncio.Imagens.Select(i => i.Url).ToList()
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                Anuncio.EhFavorito = await _db.Favoritos
                    .AnyAsync(f => f.UtilizadorId == userId && f.AnuncioId == id);
            }

            return Page();
        }

        // Substitui o antigo fetch("DELETE /api/favoritos/{id}") / POST feito em JS:
        // agora é um handler de página normal, chamado por um <form method="post">.
        public async Task<IActionResult> OnPostToggleFavoritoAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var favorito = await _db.Favoritos
                .FirstOrDefaultAsync(f => f.UtilizadorId == userId && f.AnuncioId == id);

            if (favorito != null)
                _db.Favoritos.Remove(favorito);
            else
                _db.Favoritos.Add(new Favorito { UtilizadorId = userId, AnuncioId = id });

            await _db.SaveChangesAsync();
            return RedirectToPage(new { id });
        }

        // Substitui o antigo fetch("DELETE /api/anuncios/{id}").
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var anuncio = await _db.Anuncios.FindAsync(id);

            if (anuncio == null)
                return NotFound();

            if (anuncio.VendedorId != userId)
                return Forbid();

            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();
            return RedirectToPage("/Auto/Catalogo");
        }
    }
}
