namespace AutoMarket.Models
{
    public class Favorito
    {
        public string UtilizadorId { get; set; } = string.Empty;
        public ApplicationUser? Utilizador { get; set; }

        public int AnuncioId { get; set; }
        public Anuncio? Anuncio { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}