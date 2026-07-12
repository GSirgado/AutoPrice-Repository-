using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AutoPrice.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        public string? Erro { get; set; }

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _clientFactory.CreateClient("AutoMarketAPI");
            var body = new { email = Email, password = Password };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<JsonElement>(json);
                var token = resultado.GetProperty("token").GetString();
                var nome = resultado.GetProperty("nomeCompleto").GetString();

                Response.Cookies.Append("token", token!,
                    new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Strict });
                Response.Cookies.Append("nomeCompleto", nome!);

                // Extrair o Id do utilizador a partir do JWT (só para uso do JS no chat/Mensagens,
                // não para autenticação/autorização — isso já é feito pelo JwtBearer no Program.cs)
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userId = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "sub" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    Response.Cookies.Append("userId", userId);

                // Ir buscar a foto do perfil
                var clientPerfil = _clientFactory.CreateClient("AutoMarketAPI");
                clientPerfil.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var responsePerfil = await clientPerfil.GetAsync("/api/perfil");
                if (responsePerfil.IsSuccessStatusCode)
                {
                    var jsonPerfil = await responsePerfil.Content.ReadAsStringAsync();
                    var perfil = JsonSerializer.Deserialize<JsonElement>(jsonPerfil);
                    var fotoUrl = perfil.TryGetProperty("fotoUrl", out var foto) ? foto.GetString() : null;
                    if (!string.IsNullOrEmpty(fotoUrl))
                        Response.Cookies.Append("fotoUrl", fotoUrl);
                }

                return RedirectToPage("/Index");
            }
            else
            {
                Erro = "Email ou password incorretos.";
                return Page();
            }
        }
    }
}