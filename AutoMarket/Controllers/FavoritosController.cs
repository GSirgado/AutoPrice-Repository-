using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public FavoritosController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/favoritos
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoritos = await _db.Favoritos
                .Where(f => f.UtilizadorId == userId)
                .Include(f => f.Anuncio)
                    .ThenInclude(a => a!.Imagens)
                .Include(f => f.Anuncio)
                    .ThenInclude(a => a!.Categoria)
                .OrderByDescending(f => f.CriadoEm)
                .Select(f => new
                {
                    f.AnuncioId,
                    f.CriadoEm,
                    Anuncio = f.Anuncio == null ? null : new
                    {
                        f.Anuncio.Id,
                        f.Anuncio.Titulo,
                        f.Anuncio.Marca,
                        f.Anuncio.Modelo,
                        f.Anuncio.Tipo,
                        f.Anuncio.Ano,
                        f.Anuncio.Preco,
                        f.Anuncio.Kilometragem,
                        f.Anuncio.Combustivel,
                        f.Anuncio.Condicao,
                        Imagens = f.Anuncio.Imagens.Select(i => i.Url).ToList()
                    }
                })
                .ToListAsync();

            return Ok(favoritos);
        }

        // GET api/favoritos/5/verificar
        [HttpGet("{anuncioId}/verificar")]
        public async Task<IActionResult> Verificar(int anuncioId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existe = await _db.Favoritos
                .AnyAsync(f => f.UtilizadorId == userId && f.AnuncioId == anuncioId);

            return Ok(new { favorito = existe });
        }

        // POST api/favoritos/5
        [HttpPost("{anuncioId}")]
        public async Task<IActionResult> Adicionar(int anuncioId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var anuncioExiste = await _db.Anuncios.AnyAsync(a => a.Id == anuncioId);
            if (!anuncioExiste) return NotFound("Anúncio não encontrado.");

            var jaExiste = await _db.Favoritos
                .AnyAsync(f => f.UtilizadorId == userId && f.AnuncioId == anuncioId);

            if (jaExiste) return NoContent();

            _db.Favoritos.Add(new Favorito
            {
                UtilizadorId = userId,
                AnuncioId = anuncioId
            });

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Verificar), new { anuncioId }, null);
        }

        // DELETE api/favoritos/5
        [HttpDelete("{anuncioId}")]
        public async Task<IActionResult> Remover(int anuncioId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorito = await _db.Favoritos
                .FirstOrDefaultAsync(f => f.UtilizadorId == userId && f.AnuncioId == anuncioId);

            if (favorito == null) return NotFound();

            _db.Favoritos.Remove(favorito);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}