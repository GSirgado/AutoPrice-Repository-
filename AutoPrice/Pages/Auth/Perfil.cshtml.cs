using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AutoPrice.Pages
{
    public class PerfilModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Localizacao { get; set; }
        public string? FotoUrl { get; set; }
        public List<AnuncioPerfilDto> MeusAnuncios { get; set; } = new();

        public PerfilModel(IHttpClientFactory clientFactory)
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

            var responsePerfil = await client.GetAsync("/api/perfil");
            if (responsePerfil.IsSuccessStatusCode)
            {
                var json = await responsePerfil.Content.ReadAsStringAsync();
                var perfil = JsonSerializer.Deserialize<JsonElement>(json);

                NomeCompleto = perfil.GetProperty("nomeCompleto").GetString() ?? "";
                Email = perfil.GetProperty("email").GetString() ?? "";
                Telefone = perfil.TryGetProperty("phoneNumber", out var tel) ? tel.GetString() : null;
                Localizacao = perfil.TryGetProperty("localizacao", out var loc) ? loc.GetString() : null;
                FotoUrl = perfil.TryGetProperty("fotoUrl", out var foto) ? foto.GetString() : null;
            }

            var responseAnuncios = await client.GetAsync("/api/Anuncios/meus");
            if (responseAnuncios.IsSuccessStatusCode)
            {
                var json = await responseAnuncios.Content.ReadAsStringAsync();
                MeusAnuncios = JsonSerializer.Deserialize<List<AnuncioPerfilDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return Page();
        }
    }

    public class AnuncioPerfilDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public int? Kilometragem { get; set; }
    }
}