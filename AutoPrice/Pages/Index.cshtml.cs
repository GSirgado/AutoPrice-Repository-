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

                    // ✅ CORRIGIDO: deserializar com DTO intermédio que mapeia
                    // "imagens" como lista de objetos {id, url} vindos da API
                    var anuncios = JsonSerializer.Deserialize<List<AnuncioIndexDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Destaques = anuncios?
                        .TakeLast(4)
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
                            // ✅ extrai só as URLs da lista de objetos
                            Imagens = a.Imagens?.Select(i => i.Url).ToList() ?? new()
                        })
                        .ToList() ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao chamar API: " + ex.Message);
            }
        }
    }

    // DTO intermédio para deserializar a resposta da API
    public class AnuncioIndexDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public List<AnuncioImgIndexDto>? Imagens { get; set; }
    }

    public class AnuncioImgIndexDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}