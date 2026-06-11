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
    public class AnunciosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AnunciosController(AppDbContext db)
        {
            _db = db;
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
            return Ok(anuncio);
        }

        // POST api/anuncios
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] Anuncio anuncio)
        {
            // Guardar o ID do utilizador autenticado como vendedor
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

            // Só o dono pode editar
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

            // Só o dono pode eliminar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (anuncio.VendedorId != userId)
                return Forbid();

            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}