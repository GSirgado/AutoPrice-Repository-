using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PerfilController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ObterPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.NomeCompleto,
                user.Email,
                Telefone = user.PhoneNumber,
                user.Localizacao,
                user.FotoUrl
            });
        }

        [HttpPut]
        public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            // Só atualiza os campos que foram preenchidos
            if (!string.IsNullOrEmpty(dto.NomeCompleto))
                user.NomeCompleto = dto.NomeCompleto;

            if (dto.Telefone != null)
                user.PhoneNumber = dto.Telefone;

            if (dto.Localizacao != null)
                user.Localizacao = dto.Localizacao;

            if (!string.IsNullOrEmpty(dto.FotoUrl))
                user.FotoUrl = dto.FotoUrl;

            var resultado = await _userManager.UpdateAsync(user);
            if (!resultado.Succeeded)
                return BadRequest(new { erros = resultado.Errors.Select(e => e.Description) });

            // Só altera password se ambos os campos foram preenchidos
            if (!string.IsNullOrEmpty(dto.NovaPassword) && !string.IsNullOrEmpty(dto.PasswordAtual))
            {
                var resultadoPassword = await _userManager.ChangePasswordAsync(
                    user, dto.PasswordAtual, dto.NovaPassword);

                if (!resultadoPassword.Succeeded)
                    return BadRequest(new { mensagem = "Password atual incorreta." });
            }

            return Ok(new { mensagem = "Perfil atualizado com sucesso." });
        }
    }

    public class AtualizarPerfilDto
    {
        public string? NomeCompleto { get; set; }
        public string? Telefone { get; set; }
        public string? Localizacao { get; set; }
        public string? FotoUrl { get; set; }
        public string? PasswordAtual { get; set; }
        public string? NovaPassword { get; set; }
    }
}