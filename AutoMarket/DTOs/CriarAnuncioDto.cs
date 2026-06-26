namespace AutoMarket.DTOs
{
    public class CriarAnuncioDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Carro"; // NOVO
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
        public List<string>? ImagensUrls { get; set; }
    }
}
