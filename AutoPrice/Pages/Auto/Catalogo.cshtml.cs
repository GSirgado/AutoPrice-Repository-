using AutoMarket.Data;
using AutoPrice.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoPrice.Pages
{
    public class CatalogoModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<VeiculoListaItem> Veiculos { get; set; } = new();

        public string? TipoAtual { get; set; }

        public CatalogoModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync(string? tipo)
        {
            TipoAtual = tipo;

            // A mesma filtragem que existia em GET /api/Anuncios?tipo=..., mas
            // feita diretamente na base de dados em vez de ir buscar tudo por HTTP
            // e filtrar depois.
            var query = _db.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Imagens)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(a => a.Tipo == tipo);

            Veiculos = await query
                .OrderByDescending(a => a.Id)
                .Select(a => new VeiculoListaItem
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    Ano = a.Ano,
                    Preco = a.Preco,
                    Categoria = a.Categoria != null ? a.Categoria.Nome : null,
                    Combustivel = a.Combustivel,
                    Condicao = a.Condicao,
                    Kilometragem = a.Kilometragem,
                    Tipo = a.Tipo,
                    ImagemPath = a.Imagens.Select(i => i.Url).FirstOrDefault()
                })
                .ToListAsync();
        }
    }
}
