using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnunciosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnunciosController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET api/anuncios
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var anuncios = await _db.Anuncios
                .Include(a => a.Categoria)
                .ToListAsync();
            return Ok(anuncios);
        }

        // GET api/anuncios/meus
        [HttpGet("meus")]
        [Authorize]
        public async Task<IActionResult> MeusAnuncios()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var anuncios = await _db.Anuncios
                .Include(a => a.Categoria)
                .Where(a => a.VendedorId == userId)
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.Titulo,
                    a.Marca,
                    a.Modelo,
                    a.Ano,
                    a.Preco,
                    a.Combustivel,
                    a.Condicao,
                    a.Kilometragem
                })
                .ToListAsync();

            return Ok(anuncios);
        }

        // GET api/anuncios/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.Categoria)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();

            string? nomeVendedor = null;
            if (!string.IsNullOrEmpty(anuncio.VendedorId))
            {
                var vendedor = await _userManager.FindByIdAsync(anuncio.VendedorId);
                nomeVendedor = vendedor?.NomeCompleto;
            }

            return Ok(new
            {
                anuncio.Id,
                anuncio.Titulo,
                anuncio.Descricao,
                anuncio.Marca,
                anuncio.Modelo,
                anuncio.Ano,
                anuncio.Preco,
                anuncio.Kilometragem,
                anuncio.Combustivel,
                anuncio.Condicao,
                anuncio.CategoriaId,
                anuncio.Categoria,
                anuncio.VendedorId,
                VendedorNome = nomeVendedor
            });
        }

        // POST api/anuncios
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] Anuncio anuncio)
        {
            anuncio.VendedorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            _db.Anuncios.Add(anuncio);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = anuncio.Id }, anuncio);
        }

        // PUT api/anuncios/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Anuncio dados)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (anuncio.VendedorId != userId)
                return Forbid();

            anuncio.Titulo = dados.Titulo;
            anuncio.Marca = dados.Marca;
            anuncio.Modelo = dados.Modelo;
            anuncio.Ano = dados.Ano;
            anuncio.Preco = dados.Preco;
            anuncio.Kilometragem = dados.Kilometragem;
            anuncio.Descricao = dados.Descricao;
            anuncio.Combustivel = dados.Combustivel;
            anuncio.Condicao = dados.Condicao;
            anuncio.CategoriaId = dados.CategoriaId;
            anuncio.ImagemUrl = dados.ImagemUrl;   

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/anuncios/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (anuncio.VendedorId != userId)
                return Forbid();

            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}