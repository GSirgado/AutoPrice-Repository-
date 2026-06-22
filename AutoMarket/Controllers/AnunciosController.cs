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
                .Include(a => a.Imagens)
                .ToListAsync();

            // ✅ CORRIGIDO: devolver DTO sem referências circulares
            // A API devolvia o objeto Anuncio diretamente, causando:
            // - referências circulares (Anuncio → AnuncioImg → Anuncio)
            // - campo "imagens" como lista de objetos {id, url, anuncioId}
            //   em vez de lista de strings, partindo a deserialização no frontend
            var resultado = anuncios.Select(a => new
            {
                a.Id,
                a.Titulo,
                a.Marca,
                a.Modelo,
                a.Ano,
                a.Preco,
                a.Kilometragem,
                a.Combustivel,
                a.Condicao,
                a.Cor,
                a.Transmissao,
                a.Potencia,
                a.CategoriaId,
                Categoria = a.Categoria == null ? null : new { a.Categoria.Id, a.Categoria.Nome },
                Imagens = a.Imagens.Select(i => new { i.Id, i.Url }).ToList()
            });

            return Ok(resultado);
        }

        // GET api/anuncios/meus
        [HttpGet("meus")]
        [Authorize]
        public async Task<IActionResult> MeusAnuncios()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var anuncios = await _db.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
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
                    a.Kilometragem,
                    Imagens = a.Imagens.Select(i => i.Url).ToList()
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
                .Include(a => a.Imagens)
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
                anuncio.Cor,
                anuncio.Transmissao,
                anuncio.Potencia,
                anuncio.CategoriaId,
                anuncio.Categoria,
                anuncio.VendedorId,
                VendedorNome = nomeVendedor,
                Imagens = anuncio.Imagens.Select(i => i.Url).ToList()
            });
        }

        // POST api/anuncios
        public class CriarAnuncioDto
        {
            public string Titulo { get; set; } = string.Empty;
            public string Marca { get; set; } = string.Empty;
            public string Modelo { get; set; } = string.Empty;
            public int Ano { get; set; }
            public decimal Preco { get; set; }
            public int? Kilometragem { get; set; }
            public string? Descricao { get; set; }
            public string? Combustivel { get; set; }
            public string? Condicao { get; set; }
            public string? Cor { get; set; }
            public string? Transmissao { get; set; }
            public int? Potencia { get; set; }
            public int CategoriaId { get; set; }
            public List<string>? ImagensUrls { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] CriarAnuncioDto dto)
        {
            var anuncio = new Anuncio
            {
                Titulo = dto.Titulo,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Ano = dto.Ano,
                Preco = dto.Preco,
                Kilometragem = dto.Kilometragem,
                Descricao = dto.Descricao,
                Combustivel = dto.Combustivel,
                Condicao = dto.Condicao,
                Cor = dto.Cor,
                Transmissao = dto.Transmissao,
                Potencia = dto.Potencia,
                CategoriaId = dto.CategoriaId,
                VendedorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
            };

            if (dto.ImagensUrls != null && dto.ImagensUrls.Any())
            {
                anuncio.Imagens = dto.ImagensUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => new AnuncioImg { Url = url })
                    .ToList();
            }

            _db.Anuncios.Add(anuncio);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = anuncio.Id }, anuncio);
        }

        // PUT api/anuncios/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CriarAnuncioDto dados)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == id);

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
            anuncio.Cor = dados.Cor;
            anuncio.Transmissao = dados.Transmissao;
            anuncio.Potencia = dados.Potencia;
            anuncio.CategoriaId = dados.CategoriaId;

            if (dados.ImagensUrls != null)
            {
                _db.AnuncioImagens.RemoveRange(anuncio.Imagens);
                anuncio.Imagens = dados.ImagensUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => new AnuncioImg { Url = url })
                    .ToList();
            }

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