using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AutoPrice.Pages.FavoritosPages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<FavoritoAnuncioDto> Favoritos { get; set; } = new();

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var client = _clientFactory.CreateClient("AutoMarketAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/favoritos");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var itens = JsonSerializer.Deserialize<List<FavoritoDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                Favoritos = itens.Where(i => i.Anuncio != null).Select(i => i.Anuncio!).ToList();
            }

            return Page();
        }
    }

    public class FavoritoDto
    {
        public int AnuncioId { get; set; }
        public DateTime CriadoEm { get; set; }
        public FavoritoAnuncioDto? Anuncio { get; set; }
    }

    public class FavoritoAnuncioDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public List<string> Imagens { get; set; } = new();
    }
}