namespace AutoMarket.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
    }
}