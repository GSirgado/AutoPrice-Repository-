using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<JsonElement>(json);
                var token = resultado.GetProperty("token").GetString();
                var nome = resultado.GetProperty("nomeCompleto").GetString();

                // Decodificar o JWT PRIMEIRO
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Guardar cookies
                Response.Cookies.Append("token", token!,
                    new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Strict });

                var userId = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "sub" ||
                    c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    Response.Cookies.Append("userId", userId);

                Response.Cookies.Append("nomeCompleto", nome!);

                // Claims e autenticação por cookie
                var claims = jwtToken.Claims.ToList();
                var roleClaims = claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).ToList();
                foreach (var rc in roleClaims)
                {
                    if (rc.Type != ClaimTypes.Role)
                        claims.Add(new Claim(ClaimTypes.Role, rc.Value));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = jwtToken.ValidTo
                    });

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