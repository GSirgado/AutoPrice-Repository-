namespace AutoMarket.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string Tipo { get; set; } = "Carro"; // NOVO: "Carro" ou "Mota"
        public List<Anuncio> Anuncios { get; set; } = new();
    }
}