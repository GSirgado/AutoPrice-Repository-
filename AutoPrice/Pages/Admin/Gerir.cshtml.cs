using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace AutoPrice.Pages.Admin
{
    public class GerirModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<UtilizadorAdminDto> Utilizadores { get; set; } = new();
        public List<AnuncioAdminDto> Veiculos { get; set; } = new();
        public List<CategoriaDto> Categorias { get; set; } = new();
        public string AdminId { get; set; } = "";

        public GerirModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CriarClienteAutenticado()
        {
            var token = Request.Cookies["token"];
            var client = _clientFactory.CreateClient("AutoMarketAPI");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            if (!User.IsInRole("Admin"))
                return RedirectToPage("/Index");

            var client = CriarClienteAutenticado();

            AdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var resUsers = await client.GetAsync("/api/admin/utilizadores");
            if (resUsers.IsSuccessStatusCode)
            {
                var json = await resUsers.Content.ReadAsStringAsync();
                Utilizadores = JsonSerializer.Deserialize<List<UtilizadorAdminDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var resAnuncios = await client.GetAsync("/api/admin/anuncios");
            if (resAnuncios.IsSuccessStatusCode)
            {
                var json = await resAnuncios.Content.ReadAsStringAsync();
                Veiculos = JsonSerializer.Deserialize<List<AnuncioAdminDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var resCategorias = await client.GetAsync("/api/categorias");
            if (resCategorias.IsSuccessStatusCode)
            {
                var json = await resCategorias.Content.ReadAsStringAsync();
                Categorias = JsonSerializer.Deserialize<List<CategoriaDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarUtilizadorAsync(string id)
        {
            var client = CriarClienteAutenticado();
            await client.DeleteAsync($"/api/admin/utilizadores/{id}");
            TempData["Sucesso"] = "Utilizador eliminado com sucesso.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAnuncioAsync(int id)
        {
            var client = CriarClienteAutenticado();
            await client.DeleteAsync($"/api/admin/anuncios/{id}");
            TempData["Sucesso"] = "Anúncio eliminado com sucesso.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarUtilizadorAsync(
            string id, string nomeCompleto, string email, string? localizacao, string? novaPassword)
        {
            var client = CriarClienteAutenticado();
            var body = new { nomeCompleto, email, localizacao, novaPassword };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resposta = await client.PutAsync($"/api/admin/utilizadores/{id}", content);

            if (resposta.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Utilizador atualizado com sucesso.";
            }
            else
            {
                TempData["Erro"] = "Não foi possível atualizar o utilizador. Verifica a password (mín. 6 caracteres, com pelo menos um número).";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAnuncioAsync(
            int id, string titulo, string marca, string modelo, int ano, decimal preco,
            int categoriaId, int? kilometragem, string? descricao, string? combustivel, string? condicao,
            string? imagemUrlAtual, IFormFile? novaImagem)
        {
            var client = CriarClienteAutenticado();

            string? imagemUrl = imagemUrlAtual;

            if (novaImagem != null && novaImagem.Length > 0)
            {
                using var multipart = new MultipartFormDataContent();
                using var streamContent = new StreamContent(novaImagem.OpenReadStream());
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(novaImagem.ContentType);
                multipart.Add(streamContent, "ficheiro", novaImagem.FileName);

                var uploadResponse = await client.PostAsync("/api/Upload/foto", multipart);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
                    var uploadResultado = JsonSerializer.Deserialize<JsonElement>(uploadJson);
                    imagemUrl = uploadResultado.GetProperty("url").GetString();
                }
                else
                {
                    TempData["Erro"] = "Não foi possível enviar a nova imagem.";
                    return RedirectToPage();
                }
            }

            var body = new
            {
                titulo,
                marca,
                modelo,
                ano,
                preco,
                categoriaId,
                kilometragem,
                descricao,
                combustivel,
                condicao,
                imagemUrl
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resposta = await client.PutAsync($"/api/admin/anuncios/{id}", content);

            if (resposta.IsSuccessStatusCode)
                TempData["Sucesso"] = "Anúncio atualizado com sucesso.";
            else
                TempData["Erro"] = "Não foi possível atualizar o anúncio.";

            return RedirectToPage();
        }
    }

    public class UtilizadorAdminDto
    {
        public string Id { get; set; } = "";
        public string? NomeCompleto { get; set; }
        public string? Email { get; set; }
        public string? FotoUrl { get; set; }
        public string? Localizacao { get; set; }
        public DateTime DataRegisto { get; set; }
        public int TotalAnuncios { get; set; }
    }

    public class AnuncioAdminDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public int? Kilometragem { get; set; }
        public string? Descricao { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public string? ImagemUrl { get; set; }
        public string? VendedorNome { get; set; }
        public string? VendedorEmail { get; set; }
    }

    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
    }
}