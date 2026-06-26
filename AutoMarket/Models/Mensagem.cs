namespace AutoMarket.Models
{
    public class Mensagem
    {
        public int Id { get; set; }
        public int AnuncioId { get; set; }
        public Anuncio? Anuncio { get; set; }

        public string RemetenteId { get; set; } = string.Empty;
        public ApplicationUser? Remetente { get; set; }

        public string DestinatarioId { get; set; } = string.Empty;
        public ApplicationUser? Destinatario { get; set; }

        public string Conteudo { get; set; } = string.Empty;
        public DateTime EnviadoEm { get; set; } = DateTime.UtcNow;
        public bool Lida { get; set; } = false;
    }
}