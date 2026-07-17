using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AutoPrice.Pages
{
    public class EditarPerfilModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FotoUploadService _fotoUpload;

        [BindProperty] public string? NomeCompleto { get; set; }
        [BindProperty] public string? Telefone { get; set; }
        [BindProperty] public string? Localizacao { get; set; }
        [BindProperty] public string? FotoUrl { get; set; }
        [BindProperty] public IFormFile? FotoFicheiro { get; set; }
        [BindProperty] public string? PasswordAtual { get; set; }
        [BindProperty] public string? NovaPassword { get; set; }
        [BindProperty] public string? ConfirmarPassword { get; set; }
        [BindProperty] public string? Email { get; set; }

        public string? Erro { get; set; }
        public string? Sucesso { get; set; }

        public EditarPerfilModel(UserManager<ApplicationUser> userManager, FotoUploadService fotoUpload)
        {
            _userManager = userManager;
            _fotoUpload = fotoUpload;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            await CarregarDadosAtuais();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Auth/Login");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
                return RedirectToPage("/Auth/Login");

            if (!string.IsNullOrEmpty(NovaPassword))
            {
                if (string.IsNullOrEmpty(PasswordAtual))
                {
                    Erro = "Tens de inserir a password atual para a alterar.";
                    await CarregarDadosAtuais();
                    return Page();
                }
                if (NovaPassword != ConfirmarPassword)
                {
                    Erro = "As novas passwords não coincidem.";
                    await CarregarDadosAtuais();
                    return Page();
                }
            }

            if (FotoFicheiro != null && FotoFicheiro.Length > 0)
            {
                var (sucesso, url, erro) = await _fotoUpload.UploadAsync(FotoFicheiro);
                if (!sucesso)
                {
                    Erro = $"Erro upload: {erro}";
                    await CarregarDadosAtuais();
                    return Page();
                }
                FotoUrl = url;
            }

            if (!string.IsNullOrWhiteSpace(NomeCompleto))
                user.NomeCompleto = NomeCompleto;

            if (!string.IsNullOrWhiteSpace(Email) && Email != user.Email)
            {
                var emailExiste = await _userManager.FindByEmailAsync(Email);
                if (emailExiste != null)
                {
                    Erro = "Este email já está em uso.";
                    await CarregarDadosAtuais();
                    return Page();
                }

                await _userManager.SetEmailAsync(user, Email);
                await _userManager.SetUserNameAsync(user, Email);
            }

            if (Telefone != null) user.PhoneNumber = Telefone;
            if (Localizacao != null) user.Localizacao = Localizacao;
            if (!string.IsNullOrWhiteSpace(FotoUrl)) user.FotoUrl = FotoUrl;

            var resultado = await _userManager.UpdateAsync(user);
            if (!resultado.Succeeded)
            {
                Erro = $"Erro ao atualizar: {string.Join(" ", resultado.Errors.Select(e => e.Description))}";
                await CarregarDadosAtuais();
                return Page();
            }

            if (!string.IsNullOrEmpty(NovaPassword) && !string.IsNullOrEmpty(PasswordAtual))
            {
                var resultadoPassword = await _userManager.ChangePasswordAsync(user, PasswordAtual, NovaPassword);
                if (!resultadoPassword.Succeeded)
                {
                    Erro = "Password atual incorreta.";
                    await CarregarDadosAtuais();
                    return Page();
                }
            }

            if (!string.IsNullOrEmpty(NomeCompleto))
                Response.Cookies.Append("nomeCompleto", NomeCompleto);
            if (!string.IsNullOrEmpty(FotoUrl))
                Response.Cookies.Append("fotoUrl", FotoUrl);

            Sucesso = "Perfil atualizado com sucesso!";
            await CarregarDadosAtuais();
            return Page();
        }

        private async Task CarregarDadosAtuais()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return;

            Email = user.Email;
            if (string.IsNullOrEmpty(NomeCompleto)) NomeCompleto = user.NomeCompleto;
            if (string.IsNullOrEmpty(Telefone)) Telefone = user.PhoneNumber;
            if (string.IsNullOrEmpty(Localizacao)) Localizacao = user.Localizacao;
            if (string.IsNullOrEmpty(FotoUrl)) FotoUrl = user.FotoUrl;
        }
    }
}
