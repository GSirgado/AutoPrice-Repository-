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
        [BindProperty] public string Tipo { get; set; } = "Carro";
        [BindProperty] public string? ImagensUrls { get; set; }

        public List<CategoriaItem> Categorias { get; set; } = new();
        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public VenderModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync(
            string? marca, string? modelo, int? ano,
            decimal? preco, string? combustivel,
            string? transmissao, string? condicao,
            string? tipo)
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            Tipo = tipo ?? "Carro";

            if (marca != null) Marca = marca;
            if (modelo != null) Modelo = modelo;
            if (ano.HasValue) Ano = ano.Value;
            if (preco.HasValue) Preco = preco.Value;
            if (combustivel != null) Combustivel = combustivel;
            if (transmissao != null) Transmissao = transmissao;
            if (condicao != null) Condicao = condicao;

            if (!string.IsNullOrEmpty(marca) && !string.IsNullOrEmpty(modelo) && ano.HasValue)
                Titulo = $"{marca} {modelo} {ano}";

            await CarregarCategorias(Tipo);
            return Page();
        }

        // Handler para upload de imagens via AJAX (resolve o problema do cookie HttpOnly)
        public async Task<IActionResult> OnPostUploadAsync(IFormFile ficheiro)
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return new JsonResult(new { mensagem = "Não autenticado." }) { StatusCode = 401 };

            if (ficheiro == null || ficheiro.Length == 0)
                return new JsonResult(new { mensagem = "Nenhum ficheiro enviado." }) { StatusCode = 400 };

            var client = _clientFactory.CreateClient("AutoMarketAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();
            using var streamContent = new StreamContent(ficheiro.OpenReadStream());
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(ficheiro.ContentType);
            formData.Add(streamContent, "ficheiro", ficheiro.FileName);

            var response = await client.PostAsync("/api/Upload/foto", formData);
            var json = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            await CarregarCategorias(Tipo);

            var client = _clientFactory.CreateClient("AutoMarketAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var listaImagens = string.IsNullOrEmpty(ImagensUrls)
                ? new List<string>()
                : ImagensUrls.Split(',').Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

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
                condicao = Condicao,
                cor = Cor,
                transmissao = Transmissao,
                potencia = Potencia,
                categoriaId = CategoriaId,
                imagensUrls = listaImagens
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

        private async Task CarregarCategorias(string tipo)
        {
            try
            {
                var client = _clientFactory.CreateClient("AutoMarketAPI");
                var response = await client.GetAsync("/api/Categorias");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var todas = JsonSerializer.Deserialize<List<CategoriaItem>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    if (tipo == "Mota")
                        Categorias = todas.Where(c =>
                            c.Nome.Contains("Mota") ||
                            c.Nome.Contains("Scooter") ||
                            c.Nome.Contains("Quad") ||
                            c.Nome.Contains("Minimoto")).ToList();
                    else
                        Categorias = todas.Where(c =>
                            !c.Nome.Contains("Mota") &&
                            !c.Nome.Contains("Scooter") &&
                            !c.Nome.Contains("Quad") &&
                            !c.Nome.Contains("Minimoto")).ToList();
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