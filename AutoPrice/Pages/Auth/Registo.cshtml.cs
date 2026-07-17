using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages.Auth
{
    public class RegistoModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        [BindProperty]
        public string NomeCompleto { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string? Erro { get; set; }

        public RegistoModel(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Password != ConfirmarPassword)
            {
                Erro = "As passwords não coincidem.";
                return Page();
            }

            if (await _userManager.FindByEmailAsync(Email) != null)
            {
                Erro = "Este email já está registado.";
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Email,
                Email = Email,
                NomeCompleto = NomeCompleto
            };

            // Criação do utilizador direto pelo Identity — a mesma classe que o
            // AutoMarket usa para gerir as contas, agora chamada aqui também.
            var resultado = await _userManager.CreateAsync(user, Password);
            if (!resultado.Succeeded)
            {
                Erro = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GerarToken(user, roles);

            Response.Cookies.Append("token", token,
                new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("nomeCompleto", user.NomeCompleto);
            Response.Cookies.Append("userId", user.Id);

            return RedirectToPage("/Index");
        }
    }
}
