using AutoMarket.DTOs;
using AutoMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest(new { mensagem = "Este email já está registado." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.NomeCompleto
            };

            var resultado = await _userManager.CreateAsync(user, dto.Password);
            if (!resultado.Succeeded)
            {
                var errosTraduzidos = resultado.Errors.Select(e => TraduzirErro(e.Code, e.Description));
                return BadRequest(new { erros = errosTraduzidos });
            }

            var token = GerarToken(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                NomeCompleto = user.NomeCompleto,
                Expiracao = DateTime.UtcNow.AddHours(1)
            });
        }

        private string TraduzirErro(string codigo, string mensagemOriginal)
        {
            return codigo switch
            {
                "PasswordTooShort" => "A password deve ter pelo menos 6 caracteres.",
                "PasswordRequiresNonAlphanumeric" => "A password deve conter pelo menos um caractere especial (ex: !@#$%).",
                "PasswordRequiresDigit" => "A password deve conter pelo menos um número.",
                "PasswordRequiresUpper" => "A password deve conter pelo menos uma letra maiúscula.",
                "PasswordRequiresLower" => "A password deve conter pelo menos uma letra minúscula.",
                "DuplicateUserName" => "Este email já está registado.",
                "DuplicateEmail" => "Este email já está registado.",
                "InvalidEmail" => "O email inserido não é válido.",
                "InvalidUserName" => "O nome de utilizador contém caracteres inválidos.",
                _ => mensagemOriginal
            };
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { mensagem = "Credenciais inválidas." });

            var resultado = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!resultado.Succeeded)
                return Unauthorized(new { mensagem = "Credenciais inválidas." });

            var token = GerarToken(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                NomeCompleto = user.NomeCompleto,
                Expiracao = DateTime.UtcNow.AddHours(1)
            });
        }

        private string GerarToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.NomeCompleto)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}