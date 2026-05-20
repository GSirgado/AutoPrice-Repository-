using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriasController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/categorias
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var categorias = await _db.Categorias.ToListAsync();
            return Ok(categorias);
        }

        // GET api/categorias/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return Ok(categoria);
        }

        // POST api/categorias
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Categoria categoria)
        {
            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
        }

        // PUT api/categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Categoria dados)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            categoria.Nome = dados.Nome;
            categoria.Descricao = dados.Descricao;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}