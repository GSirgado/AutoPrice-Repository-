namespace AutoPrice.ViewModels
{
    public class VeiculoListaItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public decimal Preco { get; set; }
        public decimal? PrecoAntigo { get; set; }
        public string? Categoria { get; set; }
        public string? Combustivel { get; set; }
        public string? Condicao { get; set; }
        public int? Kilometragem { get; set; }
        public string? ImagemPath { get; set; }
        public string Tipo { get; set; } = "Carro";
        public string DetalheUrl => $"/Auto/Detalhes/{Id}";
    }
}