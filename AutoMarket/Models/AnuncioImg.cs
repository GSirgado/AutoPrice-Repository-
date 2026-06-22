namespace AutoMarket.Models
{
    public class AnuncioImg
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int AnuncioId { get; set; }
        public Anuncio? Anuncio { get; set; }
    }
}