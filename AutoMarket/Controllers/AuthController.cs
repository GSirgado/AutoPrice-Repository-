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

        // POST api/auth/register
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
                return BadRequest(new { erros = resultado.Errors.Select(e => e.Description) });

            var token = GerarToken(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                NomeCompleto = user.NomeCompleto,
                Expiracao = DateTime.UtcNow.AddHours(1)
            });
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