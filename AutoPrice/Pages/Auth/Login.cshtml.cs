using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        public string? Erro { get; set; }

        public LoginModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // A validação da password passa a ser feita aqui, com o Identity a
            // consultar a base de dados diretamente — deixou de haver um pedido
            // POST /api/auth/login ao AutoMarket a fazer isto por nós.
            var user = await _userManager.FindByEmailAsync(Email);
            var loginValido = user != null &&
                (await _signInManager.CheckPasswordSignInAsync(user, Password, false)).Succeeded;

            if (!loginValido || user == null)
            {
                Erro = "Email ou password incorretos.";
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GerarToken(user, roles);

            Response.Cookies.Append("token", token,
                new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("nomeCompleto", user.NomeCompleto);
            Response.Cookies.Append("userId", user.Id);

            if (!string.IsNullOrEmpty(user.FotoUrl))
                Response.Cookies.Append("fotoUrl", user.FotoUrl);

            return RedirectToPage("/Index");
        }
    }
}
