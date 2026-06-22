namespace AutoMarket.Models
{
    public class Anuncio
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? VendedorId { get; set; }
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Descricao { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public string? Cor { get; set; }
        public string? Transmissao { get; set; }
        public int? Potencia { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Lista de imagens
        public List<AnuncioImg> Imagens { get; set; } = new();
    }
}