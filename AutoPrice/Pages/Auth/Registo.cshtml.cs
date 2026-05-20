using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace AutoPrice.Pages.Auth
{
    public class RegistoModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        [BindProperty]
        public string NomeCompleto { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string? Erro { get; set; }

        public RegistoModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Password != ConfirmarPassword)
            {
                Erro = "As passwords não coincidem.";
                return Page();
            }

            var client = _clientFactory.CreateClient("AutoMarketAPI");

            var body = new { nomeCompleto = NomeCompleto, email = Email, password = Password };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<JsonElement>(json);
                var token = resultado.GetProperty("token").GetString();
                var nome = resultado.GetProperty("nomeCompleto").GetString();

                Response.Cookies.Append("token", token!,
                    new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
                Response.Cookies.Append("nomeCompleto", nome!);

                return RedirectToPage("/Index");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Erro = "Erro ao criar conta. O email pode já estar registado.";
                return Page();
            }
        }
    }
}