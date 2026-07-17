using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages
{
    public class PerfilModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Localizacao { get; set; }
        public string? FotoUrl { get; set; }
        public List<AnuncioPerfilDto> MeusAnuncios { get; set; } = new();

        public PerfilModel(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
                return RedirectToPage("/Auth/Login");

            NomeCompleto = user.NomeCompleto;
            Email = user.Email;
            Telefone = user.PhoneNumber;
            Localizacao = user.Localizacao;
            FotoUrl = user.FotoUrl;

            // Mesma consulta que antes estava em GET /api/anuncios/meus.
            MeusAnuncios = await _db.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
                .Where(a => a.VendedorId == userId)
                .OrderByDescending(a => a.Id)
                .Select(a => new AnuncioPerfilDto
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    VendedorId = a.VendedorId,
                    Ano = a.Ano,
                    Preco = a.Preco,
                    Kilometragem = a.Kilometragem,
                    Descricao = a.Descricao,
                    Combustivel = a.Combustivel,
                    Condicao = a.Condicao,
                    CategoriaId = a.CategoriaId,
                    Categoria = a.Categoria != null ? a.Categoria.Nome : null,
                    Imagens = a.Imagens.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Page();
        }
    }

    public class AnuncioPerfilDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? VendedorId { get; set; }
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Descricao { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public int? CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public List<string> Imagens { get; set; } = new();
    }
}
