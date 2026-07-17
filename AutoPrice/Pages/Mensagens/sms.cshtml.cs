using AutoMarket.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPrice.Pages
{
    [Authorize]
    public class MensagensModel : PageModel
    {
        private readonly AppDbContext _db;

        public List<ConversaDto> Conversas { get; set; } = new();
        public List<MensagemDto> HistoricoInicial { get; set; } = new();

        // Se a página for aberta a partir de "Contactar Vendedor", vem com estes
        // parâmetros e a conversa correspondente já é aberta automaticamente.
        public int? AnuncioIdInicial { get; set; }
        public string? OutroIdInicial { get; set; }
        public string? OutroNomeInicial { get; set; }
        public string? AnuncioTituloInicial { get; set; }

        public MensagensModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync(int? anuncioId, string? outroId)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Mesma lógica de agrupamento que antes vivia em GET /api/chat/conversas,
            // continua a ser calculada aqui e não pelo SignalR — o SignalR só serve
            // para o envio/receção de mensagens em tempo real.
            var mensagens = await _db.Mensagens
                .Where(m => m.RemetenteId == meuId || m.DestinatarioId == meuId)
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .Include(m => m.Anuncio)
                .ToListAsync();

            Conversas = mensagens
                .GroupBy(m => new
                {
                    m.AnuncioId,
                    OutroId = m.RemetenteId == meuId ? m.DestinatarioId : m.RemetenteId
                })
                .Select(g =>
                {
                    var ultima = g.OrderByDescending(m => m.EnviadoEm).First();
                    var outro = ultima.RemetenteId == meuId ? ultima.Destinatario : ultima.Remetente;
                    return new ConversaDto
                    {
                        AnuncioId = g.Key.AnuncioId,
                        AnuncioTitulo = ultima.Anuncio?.Titulo ?? "Anúncio",
                        OutroId = g.Key.OutroId ?? "",
                        OutroNome = outro?.NomeCompleto ?? "Utilizador",
                        UltimaMensagem = ultima.Conteudo,
                        UltimaData = ultima.EnviadoEm,
                        NaoLidas = g.Count(m => m.DestinatarioId == meuId && !m.Lida)
                    };
                })
                .OrderByDescending(c => c.UltimaData)
                .ToList();

            if (anuncioId.HasValue && !string.IsNullOrEmpty(outroId))
            {
                AnuncioIdInicial = anuncioId;
                OutroIdInicial = outroId;

                var conversaExistente = Conversas.FirstOrDefault(c => c.AnuncioId == anuncioId && c.OutroId == outroId);
                if (conversaExistente != null)
                {
                    OutroNomeInicial = conversaExistente.OutroNome;
                    AnuncioTituloInicial = conversaExistente.AnuncioTitulo;
                }
                else
                {
                    // Ainda não há conversa (primeira mensagem a este vendedor):
                    // o nome e o título vêm diretamente do anúncio, sem precisar de
                    // fazer nenhum pedido extra à parte.
                    var anuncio = await _db.Anuncios.FindAsync(anuncioId.Value);
                    var outroUser = await _db.Users.FindAsync(outroId);
                    AnuncioTituloInicial = anuncio?.Titulo ?? "Anúncio";
                    OutroNomeInicial = outroUser?.NomeCompleto ?? "Vendedor";
                }

                HistoricoInicial = await CarregarHistoricoAsync(meuId, anuncioId.Value, outroId);
            }
        }

        private async Task<List<MensagemDto>> CarregarHistoricoAsync(string meuId, int anuncioId, string outroId)
        {
            var mensagens = await _db.Mensagens
                .Where(m => m.AnuncioId == anuncioId &&
                            ((m.RemetenteId == meuId && m.DestinatarioId == outroId) ||
                             (m.RemetenteId == outroId && m.DestinatarioId == meuId)))
                .Include(m => m.Remetente)
                .OrderBy(m => m.EnviadoEm)
                .Select(m => new MensagemDto
                {
                    Conteudo = m.Conteudo,
                    EnviadoEm = m.EnviadoEm,
                    RemetenteId = m.RemetenteId,
                    RemetenteNome = m.Remetente!.NomeCompleto
                })
                .ToListAsync();

            // Marcar como lidas as mensagens que estavam por ler, tal como o
            // GET /api/chat/{anuncioId}/{outroId} fazia.
            var naoLidas = await _db.Mensagens
                .Where(m => m.AnuncioId == anuncioId && m.RemetenteId == outroId &&
                            m.DestinatarioId == meuId && !m.Lida)
                .ToListAsync();

            if (naoLidas.Any())
            {
                naoLidas.ForEach(m => m.Lida = true);
                await _db.SaveChangesAsync();
            }

            return mensagens;
        }
    }

    public class ConversaDto
    {
        public int AnuncioId { get; set; }
        public string AnuncioTitulo { get; set; } = "";
        public string OutroId { get; set; } = "";
        public string OutroNome { get; set; } = "";
        public string UltimaMensagem { get; set; } = "";
        public DateTime UltimaData { get; set; }
        public int NaoLidas { get; set; }
    }

    public class MensagemDto
    {
        public string Conteudo { get; set; } = "";
        public DateTime EnviadoEm { get; set; }
        public string RemetenteId { get; set; } = "";
        public string RemetenteNome { get; set; } = "";
    }
}
