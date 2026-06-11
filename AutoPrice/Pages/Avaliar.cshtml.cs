using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages
{
    public class AvaliarModel : PageModel
    {
        [BindProperty] public string Marca { get; set; } = string.Empty;
        [BindProperty] public string Modelo { get; set; } = string.Empty;
        [BindProperty] public int Ano { get; set; }
        [BindProperty] public int Kilometragem { get; set; }
        [BindProperty] public string Combustivel { get; set; } = string.Empty;
        [BindProperty] public string Transmissao { get; set; } = string.Empty;
        [BindProperty] public string Condicao { get; set; } = string.Empty;
        [BindProperty] public int? Potencia { get; set; }

        public decimal? PrecoEstimado { get; set; }
        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }
        public string? Classificacao { get; set; }
        public bool Avaliado { get; set; } = false;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Base de preço por marca
            var precoBase = ObterPrecoBase(Marca, Modelo);

            // Depreciação por ano (5% ao ano, máximo 70%)
            int anosUso = DateTime.Now.Year - Ano;
            double depreciacaoAno = Math.Min(anosUso * 0.05, 0.70);

            // Depreciação por quilómetros (0.01€ por km, máximo 30%)
            double depreciacaoKm = Math.Min(Kilometragem * 0.0001, 0.30);

            // Fator de combustível
            double fatorCombustivel = Combustivel switch
            {
                "Elétrico" => 1.15,
                "Híbrido" => 1.10,
                "Gasolina" => 1.0,
                "Diesel" => 0.95,
                "GPL" => 0.85,
                _ => 1.0
            };

            // Fator de condição
            double fatorCondicao = Condicao switch
            {
                "Novo" => 1.0,
                "Como novo" => 0.90,
                "Bom estado" => 0.75,
                "Estado razoável" => 0.55,
                "Para peças" => 0.20,
                _ => 0.75
            };

            // Fator de transmissão
            double fatorTransmissao = Transmissao == "Automático" ? 1.05 : 1.0;

            // Fator de potência
            double fatorPotencia = 1.0;
            if (Potencia.HasValue)
            {
                fatorPotencia = Potencia.Value switch
                {
                    < 100 => 0.90,
                    < 150 => 1.0,
                    < 200 => 1.10,
                    < 300 => 1.20,
                    _ => 1.35
                };
            }

            // Cálculo final
            double preco = precoBase
                * (1 - depreciacaoAno)
                * (1 - depreciacaoKm)
                * fatorCombustivel
                * fatorCondicao
                * fatorTransmissao
                * fatorPotencia;

            PrecoEstimado = Math.Round((decimal)preco / 100, 0) * 100;
            PrecoMinimo = Math.Round((decimal)(preco * 0.85) / 100, 0) * 100;
            PrecoMaximo = Math.Round((decimal)(preco * 1.15) / 100, 0) * 100;

            // Classificação do negócio
            Classificacao = PrecoEstimado switch
            {
                < 3000 => "Económico",
                < 10000 => "Acessível",
                < 25000 => "Médio",
                < 50000 => "Premium",
                _ => "Luxo"
            };

            Avaliado = true;
            return Page();
        }

        private double ObterPrecoBase(string marca, string modelo)
        {
            // Preços base por marca (valor de novo aproximado)
            var precosMarca = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "BMW", 55000 }, { "Mercedes", 60000 }, { "Audi", 52000 },
                { "Volkswagen", 35000 }, { "Toyota", 32000 }, { "Honda", 28000 },
                { "Ford", 30000 }, { "Renault", 25000 }, { "Peugeot", 24000 },
                { "Citroen", 23000 }, { "Opel", 26000 }, { "Seat", 27000 },
                { "Skoda", 28000 }, { "Hyundai", 27000 }, { "Kia", 26000 },
                { "Nissan", 28000 }, { "Mazda", 29000 }, { "Volvo", 48000 },
                { "Porsche", 120000 }, { "Ferrari", 250000 }, { "Lamborghini", 220000 },
                { "Yamaha", 12000 }, { "Honda Mota", 10000 }, { "Kawasaki", 11000 },
                { "Suzuki", 9000 }, { "Ducati", 18000 }, { "KTM", 14000 },
                { "Triumph", 13000 }, { "Harley-Davidson", 22000 }
            };

            if (precosMarca.TryGetValue(marca, out double preco))
                return preco;

            return 25000; // Valor padrão
        }
    }
}