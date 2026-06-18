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
                var jsonErro = await response.Content.ReadAsStringAsync();

                try
                {
                    var erroObj = JsonSerializer.Deserialize<JsonElement>(jsonErro);

                    if (erroObj.TryGetProperty("erros", out var erros))
                    {
                        var listaErros = erros.EnumerateArray().Select(e => e.GetString()).ToList();
                        Erro = string.Join(" ", listaErros);
                    }
                    else if (erroObj.TryGetProperty("mensagem", out var mensagem))
                    {
                        Erro = mensagem.GetString();
                    }
                    else
                    {
                        Erro = "Erro ao criar conta.";
                    }
                }
                catch
                {
                    Erro = "Erro ao criar conta. O email pode já estar registado.";
                }

                return Page();
            }
        }
    }
}