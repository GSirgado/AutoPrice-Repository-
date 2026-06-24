using AutoMarket.Data;
using AutoMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace AutoMarket.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Entrar na sala do anúncio (qualquer um dos dois utilizadores pode chamar este método)
        public async Task EntrarNaSala(int anuncioId, string outroUserId)
        {
            var meuId = Context.UserIdentifier!;
            var salaId = GerarSalaId(anuncioId, meuId, outroUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, salaId);
        }

        // Enviar mensagem
        public async Task EnviarMensagem(int anuncioId, string destinatarioId, string conteudo)
        {
            var remetenteId = Context.UserIdentifier!;
            var remetente = await _userManager.FindByIdAsync(remetenteId);

            var salaId = GerarSalaId(anuncioId, remetenteId, destinatarioId);

            var mensagem = new Mensagem
            {
                AnuncioId = anuncioId,
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                Conteudo = conteudo,
                EnviadoEm = DateTime.UtcNow
            };

            _db.Mensagens.Add(mensagem);
            await _db.SaveChangesAsync();

            await Clients.Group(salaId).SendAsync("ReceberMensagem", new
            {
                mensagem.Id,
                mensagem.AnuncioId,
                mensagem.Conteudo,
                mensagem.EnviadoEm,
                RemetenteId = remetenteId,
                RemetenteNome = remetente?.NomeCompleto ?? "Utilizador"
            });
        }

        // Gera um ID de sala único por anúncio + par de utilizadores (ordem não importa)
        private static string GerarSalaId(int anuncioId, string userId1, string userId2)
        {
            var ordenados = new[] { userId1, userId2 }.OrderBy(x => x).ToArray();
            return $"anuncio_{anuncioId}_{ordenados[0]}_{ordenados[1]}";
        }
    }
}