using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace AutoPrice.Pages
{
    public class EditarPerfilModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        [BindProperty] public string? NomeCompleto { get; set; }
        [BindProperty] public string? Telefone { get; set; }
        [BindProperty] public string? Localizacao { get; set; }
        [BindProperty] public string? FotoUrl { get; set; }
        [BindProperty] public IFormFile? FotoFicheiro { get; set; }
        [BindProperty] public string? PasswordAtual { get; set; }
        [BindProperty] public string? NovaPassword { get; set; }
        [BindProperty] public string? ConfirmarPassword { get; set; }

        public string? Email { get; set; }
        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public EditarPerfilModel(IHttpClientFactory clientFactory)
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

            var response = await client.GetAsync("/api/perfil");
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(NomeCompleto))
                    Response.Cookies.Append("nomeCompleto", NomeCompleto);

                // Guardar foto no cookie
                if (!string.IsNullOrEmpty(FotoUrl))
                    Response.Cookies.Append("fotoUrl", FotoUrl);

                Sucesso = "Perfil atualizado com sucesso!";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            // Validar password
            if (!string.IsNullOrEmpty(NovaPassword))
            {
                if (string.IsNullOrEmpty(PasswordAtual))
                {
                    Erro = "Tens de inserir a password atual para a alterar.";
                    await CarregarEmail(token);
                    return Page();
                }
                if (NovaPassword != ConfirmarPassword)
                {
                    Erro = "As novas passwords não coincidem.";
                    await CarregarEmail(token);
                    return Page();
                }
            }

            var client = _clientFactory.CreateClient("AutoMarketAPI");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Upload de foto se foi enviada
            if (FotoFicheiro != null && FotoFicheiro.Length > 0)
            {
                using var formData = new MultipartFormDataContent();
                var fileContent = new StreamContent(FotoFicheiro.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(FotoFicheiro.ContentType);
                formData.Add(fileContent, "ficheiro", FotoFicheiro.FileName);

                var responseUpload = await client.PostAsync("/api/upload/foto", formData);
                if (responseUpload.IsSuccessStatusCode)
                {
                    var jsonUpload = await responseUpload.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<JsonElement>(jsonUpload);
                    FotoUrl = resultado.GetProperty("url").GetString();
                }
                else
                {
                    var erroDetalhe = await responseUpload.Content.ReadAsStringAsync();
                    Erro = $"Erro upload: {responseUpload.StatusCode} - {erroDetalhe}";
                    await CarregarEmail(token);
                    return Page();
                }
            }

            // Enviar apenas o que foi preenchido
            var body = new
            {
                nomeCompleto = string.IsNullOrWhiteSpace(NomeCompleto) ? null : NomeCompleto,
                telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone,
                localizacao = string.IsNullOrWhiteSpace(Localizacao) ? null : Localizacao,
                fotoUrl = string.IsNullOrWhiteSpace(FotoUrl) ? null : FotoUrl,
                passwordAtual = string.IsNullOrWhiteSpace(PasswordAtual) ? null : PasswordAtual,
                novaPassword = string.IsNullOrWhiteSpace(NovaPassword) ? null : NovaPassword
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync("/api/perfil", content);

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(NomeCompleto))
                    Response.Cookies.Append("nomeCompleto", NomeCompleto);

                Sucesso = "Perfil atualizado com sucesso!";
                await CarregarEmail(token);
            }
            else
            {
                var erroJson = await response.Content.ReadAsStringAsync();
                Erro = $"Erro ao atualizar: {erroJson}";
                await CarregarEmail(token);
            }

            return Page();
        }

        private async Task CarregarEmail(string token)
        {
            try
            {
                var client = _clientFactory.CreateClient("AutoMarketAPI");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("/api/perfil");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var perfil = JsonSerializer.Deserialize<JsonElement>(json);
                    Email = perfil.GetProperty("email").GetString() ?? "";

                    // Repor campos com os valores atuais se estiverem vazios
                    if (string.IsNullOrEmpty(NomeCompleto))
                        NomeCompleto = perfil.GetProperty("nomeCompleto").GetString() ?? "";
                    if (string.IsNullOrEmpty(Telefone))
                        Telefone = perfil.TryGetProperty("telefone", out var tel) ? tel.GetString() : null;
                    if (string.IsNullOrEmpty(Localizacao))
                        Localizacao = perfil.TryGetProperty("localizacao", out var loc) ? loc.GetString() : null;
                    if (string.IsNullOrEmpty(FotoUrl))
                        FotoUrl = perfil.TryGetProperty("fotoUrl", out var foto) ? foto.GetString() : null;
                }
            }
            catch { }
        }
    }
}