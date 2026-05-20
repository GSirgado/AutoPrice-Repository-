using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Criar([FromBody] Anuncio anuncio)
        {
            _db.Anuncios.Add(anuncio);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = anuncio.Id }, anuncio);
        }

        // PUT api/anuncios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Anuncio dados)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

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
        public async Task<IActionResult> Eliminar(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}