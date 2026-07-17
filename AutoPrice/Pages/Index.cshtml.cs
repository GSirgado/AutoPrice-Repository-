using AutoMarket.Data;
using AutoPrice.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoPrice.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<AnuncioView> Destaques { get; set; } = new();

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            // Antes: GET /api/Anuncios ao AutoMarket + TakeLast(4) em memória.
            // Agora vai direto à BD e já traz só os últimos 4 anúncios.
            Destaques = await _db.Anuncios
                .Include(a => a.Imagens)
                .OrderByDescending(a => a.Id)
                .Take(4)
                .Select(a => new AnuncioView
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    Ano = a.Ano,
                    Preco = a.Preco,
                    Combustivel = a.Combustivel,
                    Condicao = a.Condicao,
                    Kilometragem = a.Kilometragem,
                    Imagens = a.Imagens.Select(i => i.Url).ToList()
                })
                .ToListAsync();
        }
    }
}
