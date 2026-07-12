using AutoPrice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AutoPrice.Pages.AutoPages
{
    public class DetalhesModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public AnuncioView? Anuncio { get; set; }

        public DetalhesModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("AutoMarketAPI");
                var response = await client.GetAsync($"/api/Anuncios/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Anuncio = JsonSerializer.Deserialize<AnuncioView>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var token = Request.Cookies["token"];
                    if (Anuncio != null && !string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var respFav = await client.GetAsync($"/api/favoritos/{id}/verificar");
                        if (respFav.IsSuccessStatusCode)
                        {
                            var jsonFav = await respFav.Content.ReadAsStringAsync();
                            var favObj = JsonSerializer.Deserialize<JsonElement>(jsonFav);
                            Anuncio.EhFavorito = favObj.TryGetProperty("favorito", out var f) && f.GetBoolean();
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }

            return Page();
        }
    }
}