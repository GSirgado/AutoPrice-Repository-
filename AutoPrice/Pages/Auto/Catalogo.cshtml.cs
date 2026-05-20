using AutoPrice.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AutoPrice.Pages
{
    public class CatalogoModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<VeiculoListaItem> Veiculos { get; set; } = new();

        public CatalogoModel(IHttpClientFactory clientFactory)
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
                    var anuncios = JsonSerializer.Deserialize<List<AnuncioApiDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Veiculos = anuncios?.Select(a => new VeiculoListaItem
                    {
                        Id = a.Id,
                        Titulo = a.Titulo,
                        Marca = a.Marca,
                        Modelo = a.Modelo,
                        Ano = a.Ano,
                        Preco = a.Preco,
                        Categoria = a.Categoria?.Nome,
                        Combustivel = a.Combustivel,
                        Condicao = a.Condicao,
                        Kilometragem = a.Kilometragem,
                        Tipo = a.Categoria?.Nome ?? "Carro"
                    }).ToList() ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }
        }
    }

    // DTO para deserializar a resposta da API
    public class AnuncioApiDto
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
        public CategoriaApiDto? Categoria { get; set; }
    }

    public class CategoriaApiDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}