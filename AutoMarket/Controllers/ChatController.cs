using AutoMarket.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ChatController(AppDbContext db) => _db = db;

        // GET api/chat/conversas
        [HttpGet("conversas")]
        public async Task<IActionResult> Conversas()
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var conversas = await _db.Mensagens
                .Where(m => m.RemetenteId == meuId || m.DestinatarioId == meuId)
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .Include(m => m.Anuncio)
                .ToListAsync();

            var agrupadas = conversas
                .GroupBy(m => new {
                    m.AnuncioId,
                    OutroId = m.RemetenteId == meuId ? m.DestinatarioId : m.RemetenteId
                })
                .Select(g => {
                    var ultima = g.OrderByDescending(m => m.EnviadoEm).First();
                    var outro = ultima.RemetenteId == meuId ? ultima.Destinatario : ultima.Remetente;
                    return new
                    {
                        AnuncioId = g.Key.AnuncioId,
                        AnuncioTitulo = ultima.Anuncio?.Titulo ?? "Anúncio",
                        OutroId = g.Key.OutroId,
                        OutroNome = outro?.NomeCompleto ?? "Utilizador",
                        UltimaMensagem = ultima.Conteudo,
                        UltimaData = ultima.EnviadoEm,
                        NaoLidas = g.Count(m => m.DestinatarioId == meuId && !m.Lida)
                    };
                })
                .OrderByDescending(c => c.UltimaData)
                .ToList();

            return Ok(agrupadas);
        }

        // GET api/chat/{anuncioId}/{outroUserId}
        [HttpGet("{anuncioId}/{outroUserId}")]
        public async Task<IActionResult> Historico(int anuncioId, string outroUserId)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var mensagens = await _db.Mensagens
                .Where(m => m.AnuncioId == anuncioId &&
                            ((m.RemetenteId == meuId && m.DestinatarioId == outroUserId) ||
                             (m.RemetenteId == outroUserId && m.DestinatarioId == meuId)))
                .Include(m => m.Remetente)
                .OrderBy(m => m.EnviadoEm)
                .Select(m => new {
                    m.Id,
                    m.Conteudo,
                    m.EnviadoEm,
                    m.Lida,
                    RemetenteId = m.RemetenteId,
                    RemetenteNome = m.Remetente!.NomeCompleto
                })
                .ToListAsync();

            var naoLidas = await _db.Mensagens
                .Where(m => m.AnuncioId == anuncioId &&
                            m.RemetenteId == outroUserId &&
                            m.DestinatarioId == meuId && !m.Lida)
                .ToListAsync();

            if (naoLidas.Any())
            {
                naoLidas.ForEach(m => m.Lida = true);
                await _db.SaveChangesAsync();
            }

            return Ok(mensagens);
        }
    }
}