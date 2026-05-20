using AutoPrice.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AutoPrice.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<AnuncioView> Destaques { get; set; } = new();

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("AutoMarketAPI");
                var response = await client.GetAsync("/api/Anuncios");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var anuncios = JsonSerializer.Deserialize<List<AnuncioView>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Mostra os últimos 6 anúncios
                    Destaques = anuncios?.TakeLast(4).ToList() ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao chamar API: " + ex.Message);
            }
        }
    }
}