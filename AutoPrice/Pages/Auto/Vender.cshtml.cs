using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace AutoPrice.Pages
{
    public class VenderModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

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

        public List<CategoriaItem> Categorias { get; set; } = new();
        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public VenderModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task OnGetAsync()
        {
            await CarregarCategorias();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar se o utilizador está autenticado
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            await CarregarCategorias();

            var client = _clientFactory.CreateClient("AutoMarketAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                titulo = Titulo,
                marca = Marca,
                modelo = Modelo,
                ano = Ano,
                preco = Preco,
                kilometragem = Kilometragem,
                descricao = Descricao,
                combustivel = Combustivel,
                transmissao = Transmissao,
                cor = Cor,
                potencia = Potencia,
                condicao = Condicao,
                localizacao = Localizacao,
                categoriaId = CategoriaId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Anuncios", content);

            if (response.IsSuccessStatusCode)
            {
                Sucesso = "Anúncio publicado com sucesso!";
                return Page();
            }
            else
            {
                Erro = "Erro ao publicar o anúncio. Tente novamente.";
                return Page();
            }
        }

        private async Task CarregarCategorias()
        {
            try
            {
                var client = _clientFactory.CreateClient("AutoMarketAPI");
                var response = await client.GetAsync("/api/Categorias");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Categorias = JsonSerializer.Deserialize<List<CategoriaItem>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar categorias: " + ex.Message);
            }
        }
    }

    public class CategoriaItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}