using AutoMarket.Data;
using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages
{
    public class VenderModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly FotoUploadService _fotoUpload;

        [BindProperty] public string Titulo { get; set; } = string.Empty;
        [BindProperty] public string Marca { get; set; } = string.Empty;
        [BindProperty] public string Modelo { get; set; } = string.Empty;
        [BindProperty] public int Ano { get; set; }
        [BindProperty] public decimal Preco { get; set; }
        [BindProperty] public int? Kilometragem { get; set; }
        [BindProperty] public string? Descricao { get; set; }
        [BindProperty] public string? Combustivel { get; set; }
        [BindProperty] public string? Transmissao { get; set; }
        [BindProperty] public string? Cor { get; set; }
        [BindProperty] public int? Potencia { get; set; }
        [BindProperty] public string? Condicao { get; set; }
        [BindProperty] public string? Localizacao { get; set; }
        [BindProperty] public int CategoriaId { get; set; }
        [BindProperty] public string Tipo { get; set; } = "Carro";

        // Imagens já publicadas que continuam associadas ao anúncio (usado em modo edição
        // e para as fotos que o utilizador acabou de adicionar mas ainda não foram gravadas).
        [BindProperty] public string? ImagensUrls { get; set; }

        // Ficheiros novos escolhidos no formulário — só são enviados para o Storage
        // quando o formulário é mesmo submetido, não a cada foto selecionada.
        [BindProperty] public List<IFormFile>? NovosFicheiros { get; set; }

        [BindProperty] public int? AnuncioId { get; set; }

        public bool ModoEdicao => AnuncioId.HasValue;
        public List<CategoriaItem> Categorias { get; set; } = new();
        public List<CategoriaItem> TodasCategorias { get; set; } = new();
        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public VenderModel(AppDbContext db, FotoUploadService fotoUpload)
        {
            _db = db;
            _fotoUpload = fotoUpload;
        }

        public async Task<IActionResult> OnGetAsync(int? id, string? marca, string? modelo, int? ano,
            decimal? preco, string? combustivel, string? transmissao, string? condicao, string? tipo)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            Tipo = tipo ?? "Carro";

            if (id.HasValue)
            {
                AnuncioId = id;

                var anuncio = await _db.Anuncios
                    .Include(a => a.Imagens)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (anuncio != null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var isAdmin = User.IsInRole("Admin");

                    if (anuncio.VendedorId != userId && !isAdmin)
                        return RedirectToPage("/Index");

                    Titulo = anuncio.Titulo;
                    Marca = anuncio.Marca;
                    Modelo = anuncio.Modelo;
                    Ano = anuncio.Ano;
                    Preco = anuncio.Preco;
                    Kilometragem = anuncio.Kilometragem;
                    Descricao = anuncio.Descricao;
                    Combustivel = anuncio.Combustivel;
                    Transmissao = anuncio.Transmissao;
                    Cor = anuncio.Cor;
                    Potencia = anuncio.Potencia;
                    Condicao = anuncio.Condicao;
                    CategoriaId = anuncio.CategoriaId;
                    Tipo = anuncio.Tipo;
                    ImagensUrls = string.Join(",", anuncio.Imagens.Select(i => i.Url));
                }
            }
            else
            {
                if (marca != null) Marca = marca;
                if (modelo != null) Modelo = modelo;
                if (ano.HasValue) Ano = ano.Value;
                if (preco.HasValue) Preco = preco.Value;
                if (combustivel != null) Combustivel = combustivel;
                if (transmissao != null) Transmissao = transmissao;
                if (condicao != null) Condicao = condicao;
                if (!string.IsNullOrEmpty(marca) && !string.IsNullOrEmpty(modelo) && ano.HasValue)
                    Titulo = $"{marca} {modelo} {ano}";
            }

            await CarregarCategorias();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            await CarregarCategorias();

            // Fotos que já estavam publicadas (mantidas pelo utilizador no formulário).
            var listaImagens = string.IsNullOrEmpty(ImagensUrls)
                ? new List<string>()
                : ImagensUrls.Split(',').Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

            // Só agora, ao submeter o formulário, é que as fotos novas vão para o
            // Storage — antes disso ficaram só como pré-visualização local no browser.
            if (NovosFicheiros != null)
            {
                foreach (var ficheiro in NovosFicheiros)
                {
                    var (sucesso, url, erro) = await _fotoUpload.UploadAsync(ficheiro);
                    if (!sucesso)
                    {
                        Erro = $"Erro ao publicar fotos: {erro}";
                        return Page();
                    }
                    listaImagens.Add(url!);
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (AnuncioId.HasValue)
            {
                var anuncio = await _db.Anuncios
                    .Include(a => a.Imagens)
                    .FirstOrDefaultAsync(a => a.Id == AnuncioId);

                if (anuncio == null) return NotFound();
                if (anuncio.VendedorId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                anuncio.Titulo = Titulo;
                anuncio.Marca = Marca;
                anuncio.Modelo = Modelo;
                anuncio.Tipo = Tipo;
                anuncio.Ano = Ano;
                anuncio.Preco = Preco;
                anuncio.Kilometragem = Kilometragem;
                anuncio.Descricao = Descricao;
                anuncio.Combustivel = Combustivel;
                anuncio.Condicao = Condicao;
                anuncio.Cor = Cor;
                anuncio.Transmissao = Transmissao;
                anuncio.Potencia = Potencia;
                anuncio.CategoriaId = CategoriaId;

                _db.AnuncioImagens.RemoveRange(anuncio.Imagens);
                anuncio.Imagens = listaImagens.Select(u => new AnuncioImg { Url = u }).ToList();

                Sucesso = "Anúncio atualizado com sucesso!";
            }
            else
            {
                var anuncio = new Anuncio
                {
                    Titulo = Titulo,
                    Marca = Marca,
                    Modelo = Modelo,
                    Tipo = Tipo,
                    Ano = Ano,
                    Preco = Preco,
                    Kilometragem = Kilometragem,
                    Descricao = Descricao,
                    Combustivel = Combustivel,
                    Condicao = Condicao,
                    Cor = Cor,
                    Transmissao = Transmissao,
                    Potencia = Potencia,
                    CategoriaId = CategoriaId,
                    VendedorId = userId,
                    Imagens = listaImagens.Select(u => new AnuncioImg { Url = u }).ToList()
                };

                _db.Anuncios.Add(anuncio);
                Sucesso = "Anúncio publicado com sucesso!";
            }

            await _db.SaveChangesAsync();

            // Devolve a lista atualizada de imagens à vista, para o formulário não
            // "esquecer" as fotos que acabaram de ser enviadas.
            ImagensUrls = string.Join(",", listaImagens);
            return Page();
        }

        private async Task CarregarCategorias()
        {
            TodasCategorias = await _db.Categorias
                .Select(c => new CategoriaItem { Id = c.Id, Nome = c.Nome, Tipo = c.Tipo })
                .ToListAsync();
            Categorias = TodasCategorias.Where(c => c.Tipo == Tipo).ToList();
        }
    }

    public class CategoriaItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Carro";
    }
}
