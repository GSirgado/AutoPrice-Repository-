using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public DateTime DataRegisto { get; set; } = DateTime.UtcNow;
        public string? FotoUrl { get; set; }
        public string? Localizacao { get; set; }

    }
}