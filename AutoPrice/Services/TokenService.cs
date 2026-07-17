using AutoMarket.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPrice.Services
{
    public interface ITokenService
    {
        string GerarToken(ApplicationUser user, IList<string> roles);
    }

    // Gera o mesmo tipo de JWT que antes era emitido pelo AuthController do
    // AutoMarket. Continua a existir porque o token ainda é usado para autenticar
    // a ligação em tempo real ao chat (SignalR), que corre no processo do
    // AutoMarket — para tudo o resto (páginas, [Authorize] neste projeto), o token
    // guardado no cookie já chega, sem ser preciso pedir nada a mais ninguém.
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GerarToken(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.NomeCompleto)
            };

            foreach (var role in roles)
                claims.Add(new Claim("role", role));

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
