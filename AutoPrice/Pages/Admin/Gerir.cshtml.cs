using AutoMarket.Data;
using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages.Admin
{
    public class GerirModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FotoUploadService _fotoUpload;

        public List<UtilizadorAdminDto> Utilizadores { get; set; } = new();
        public List<AnuncioAdminDto> Veiculos { get; set; } = new();
        public List<CategoriaDto> Categorias { get; set; } = new();
        public string AdminId { get; set; } = "";

        public GerirModel(AppDbContext db, UserManager<ApplicationUser> userManager, FotoUploadService fotoUpload)
        {
            _db = db;
            _userManager = userManager;
            _fotoUpload = fotoUpload;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            if (!User.IsInRole("Admin"))
                return RedirectToPage("/Index");

            AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            // As três consultas que antes viviam em GET /api/admin/utilizadores,
            // GET /api/admin/anuncios e GET /api/categorias, feitas diretamente aqui.
            Utilizadores = await _db.Users
                .Select(u => new UtilizadorAdminDto
                {
                    Id = u.Id,
                    NomeCompleto = u.NomeCompleto,
                    Email = u.Email,
                    FotoUrl = u.FotoUrl,
                    Localizacao = u.Localizacao,
                    DataRegisto = u.DataRegisto,
                    TotalAnuncios = _db.Anuncios.Count(a => a.VendedorId == u.Id)
                })
                .ToListAsync();

            var anuncios = await _db.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
                .ToListAsync();

            var users = await _db.Users.ToDictionaryAsync(u => u.Id, u => u);

            Veiculos = anuncios.Select(a => new AnuncioAdminDto
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Marca = a.Marca,
                Modelo = a.Modelo,
                Ano = a.Ano,
                Preco = a.Preco,
                CategoriaId = a.CategoriaId,
                Categoria = a.Categoria?.Nome,
                Kilometragem = a.Kilometragem,
                Descricao = a.Descricao,
                Combustivel = a.Combustivel,
                Condicao = a.Condicao,
                Imagens = a.Imagens.Select(i => i.Url).ToList(),
                VendedorNome = a.VendedorId != null && users.ContainsKey(a.VendedorId) ? users[a.VendedorId].NomeCompleto : null,
                VendedorEmail = a.VendedorId != null && users.ContainsKey(a.VendedorId) ? users[a.VendedorId].Email : null
            }).ToList();

            Categorias = await _db.Categorias
                .Select(c => new CategoriaDto { Id = c.Id, Nome = c.Nome })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarUtilizadorAsync(string id)
        {
            var utilizador = await _db.Users.FindAsync(id);
            if (utilizador != null)
            {
                _db.Users.Remove(utilizador);
                await _db.SaveChangesAsync();
            }

            TempData["Sucesso"] = "Utilizador eliminado com sucesso.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAnuncioAsync(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio != null)
            {
                _db.Anuncios.Remove(anuncio);
                await _db.SaveChangesAsync();
            }

            TempData["Sucesso"] = "Anúncio eliminado com sucesso.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarUtilizadorAsync(
            string id, string nomeCompleto, string email, string? localizacao, string? novaPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Erro"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            user.NomeCompleto = nomeCompleto;
            user.Localizacao = localizacao;

            if (!string.IsNullOrEmpty(email) && !string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                await _userManager.SetEmailAsync(user, email);
                await _userManager.SetUserNameAsync(user, email);
            }

            if (!string.IsNullOrWhiteSpace(novaPassword))
            {
                await _userManager.RemovePasswordAsync(user);
                var addResult = await _userManager.AddPasswordAsync(user, novaPassword);
                if (!addResult.Succeeded)
                {
                    TempData["Erro"] = "Não foi possível atualizar o utilizador. Verifica a password (mín. 6 caracteres, com pelo menos um número).";
                    return RedirectToPage();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            TempData[result.Succeeded ? "Sucesso" : "Erro"] = result.Succeeded
                ? "Utilizador atualizado com sucesso."
                : "Não foi possível atualizar o utilizador. Verifica a password (mín. 6 caracteres, com pelo menos um número).";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAnuncioAsync(
            int id, string titulo, string marca, string modelo, int ano, decimal preco,
            int categoriaId, int? kilometragem, string? descricao, string? combustivel, string? condicao,
            string? imagensAtuais, IFormFile? novaImagem)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null)
            {
                TempData["Erro"] = "Anúncio não encontrado.";
                return RedirectToPage();
            }

            // imagensAtuais chega como URLs separados por vírgula (campo hidden no formulário)
            var imagensUrls = string.IsNullOrWhiteSpace(imagensAtuais)
                ? new List<string>()
                : imagensAtuais.Split(',').Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

            if (novaImagem != null && novaImagem.Length > 0)
            {
                var (sucesso, url, erro) = await _fotoUpload.UploadAsync(novaImagem);
                if (!sucesso)
                {
                    TempData["Erro"] = "Não foi possível enviar a nova imagem.";
                    return RedirectToPage();
                }
                imagensUrls.Add(url!);
            }

            anuncio.Titulo = titulo;
            anuncio.Marca = marca;
            anuncio.Modelo = modelo;
            anuncio.Ano = ano;
            anuncio.Preco = preco;
            anuncio.CategoriaId = categoriaId;
            anuncio.Kilometragem = kilometragem;
            anuncio.Descricao = descricao;
            anuncio.Combustivel = combustivel;
            anuncio.Condicao = condicao;

            _db.AnuncioImagens.RemoveRange(anuncio.Imagens);
            anuncio.Imagens = imagensUrls.Select(u => new AnuncioImg { Url = u }).ToList();

            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Anúncio atualizado com sucesso.";
            return RedirectToPage();
        }
    }

    public class UtilizadorAdminDto
    {
        public string Id { get; set; } = "";
        public string? NomeCompleto { get; set; }
        public string? Email { get; set; }
        public string? FotoUrl { get; set; }
        public string? Localizacao { get; set; }
        public DateTime DataRegisto { get; set; }
        public int TotalAnuncios { get; set; }
    }

    public class AnuncioAdminDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public int? Kilometragem { get; set; }
        public string? Descricao { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public List<string> Imagens { get; set; } = new();
        public string? VendedorNome { get; set; }
        public string? VendedorEmail { get; set; }
    }

    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
    }
}
