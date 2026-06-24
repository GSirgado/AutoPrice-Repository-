namespace AutoPrice.Models
{
    public class AnuncioView
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public int? Kilometragem { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public string? Descricao { get; set; }
        public string? Cor { get; set; }
        public int? Potencia { get; set; }
        public string? Transmissao { get; set; }
        public string? CategoriaNome { get; set; }
        public string? VendedorNome { get; set; }
        public string? VendedorId { get; set; }    
        public List<string> Imagens { get; set; } = new();
    }
}