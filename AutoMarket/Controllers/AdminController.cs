using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("utilizadores")]
        public async Task<IActionResult> GetUtilizadores()
        {
            var utilizadores = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.NomeCompleto,
                    u.Email,
                    u.FotoUrl,
                    u.Localizacao,
                    u.DataRegisto,
                    TotalAnuncios = _context.Anuncios.Count(a => a.VendedorId == u.Id)
                })
                .ToListAsync();

            return Ok(utilizadores);
        }

        [HttpGet("anuncios")]
        public async Task<IActionResult> GetAnuncios()
        {
            var anuncios = await _context.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
                .ToListAsync();

            var users = await _context.Users.ToDictionaryAsync(u => u.Id, u => u);

            var resultado = anuncios.Select(a => new
            {
                a.Id,
                a.Titulo,
                a.Marca,
                a.Modelo,
                a.Ano,
                a.Preco,
                a.CategoriaId,
                Categoria = a.Categoria != null ? a.Categoria.Nome : null,
                a.Kilometragem,
                a.Descricao,
                a.Combustivel,
                a.Condicao,
                Imagens = a.Imagens.Select(i => i.Url).ToList(),
                VendedorNome = a.VendedorId != null && users.ContainsKey(a.VendedorId)
                    ? users[a.VendedorId].NomeCompleto : null,
                VendedorEmail = a.VendedorId != null && users.ContainsKey(a.VendedorId)
                    ? users[a.VendedorId].Email : null
            });

            return Ok(resultado);
        }

        [HttpPut("utilizadores/{id}")]
        public async Task<IActionResult> EditarUtilizador(string id, [FromBody] EditarUtilizadorRequest req)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.NomeCompleto = req.NomeCompleto;
            user.Localizacao = req.Localizacao;

            if (!string.IsNullOrEmpty(req.Email) &&
                !string.Equals(req.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, req.Email);
                if (!setEmail.Succeeded) return BadRequest(setEmail.Errors);

                var setUserName = await _userManager.SetUserNameAsync(user, req.Email);
                if (!setUserName.Succeeded) return BadRequest(setUserName.Errors);
            }

            if (!string.IsNullOrWhiteSpace(req.NovaPassword))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

                var addResult = await _userManager.AddPasswordAsync(user, req.NovaPassword);
                if (!addResult.Succeeded) return BadRequest(addResult.Errors);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPut("anuncios/{id}")]
        public async Task<IActionResult> EditarAnuncio(int id, [FromBody] EditarAnuncioRequest req)
        {
            var anuncio = await _context.Anuncios
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null) return NotFound();

            anuncio.Titulo = req.Titulo;
            anuncio.Marca = req.Marca;
            anuncio.Modelo = req.Modelo;
            anuncio.Ano = req.Ano;
            anuncio.Preco = req.Preco;
            anuncio.CategoriaId = req.CategoriaId;
            anuncio.Kilometragem = req.Kilometragem;
            anuncio.Descricao = req.Descricao;
            anuncio.Combustivel = req.Combustivel;
            anuncio.Condicao = req.Condicao;

            if (req.ImagensUrls != null)
            {
                _context.AnuncioImagens.RemoveRange(anuncio.Imagens);
                anuncio.Imagens = req.ImagensUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => new AnuncioImg { Url = url })
                    .ToList();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("utilizadores/{id}")]
        public async Task<IActionResult> EliminarUtilizador(string id)
        {
            var utilizador = await _context.Users.FindAsync(id);
            if (utilizador == null) return NotFound();

            _context.Users.Remove(utilizador);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("anuncios/{id}")]
        public async Task<IActionResult> EliminarAnuncio(int id)
        {
            var anuncio = await _context.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            _context.Anuncios.Remove(anuncio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class EditarUtilizadorRequest
    {
        public string NomeCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Localizacao { get; set; }
        public string? NovaPassword { get; set; }
    }

    public class EditarAnuncioRequest
    {
        public string Titulo { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int CategoriaId { get; set; }
        public int? Kilometragem { get; set; }
        public string? Descricao { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public List<string>? ImagensUrls { get; set; }
    }
}