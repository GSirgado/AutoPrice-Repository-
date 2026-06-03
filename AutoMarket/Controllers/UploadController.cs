using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("foto")]
        public async Task<IActionResult> UploadFoto(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return BadRequest(new { mensagem = "Nenhum ficheiro enviado." });

            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!tiposPermitidos.Contains(ficheiro.ContentType))
                return BadRequest(new { mensagem = "Tipo de ficheiro não permitido." });

            if (ficheiro.Length > 5 * 1024 * 1024)
                return BadRequest(new { mensagem = "Ficheiro demasiado grande. Máximo 5MB." });

            // Usar ContentRootPath em vez de WebRootPath
            var pastaUploads = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "fotos");
            if (!Directory.Exists(pastaUploads))
                Directory.CreateDirectory(pastaUploads);

            var extensao = Path.GetExtension(ficheiro.FileName);
            var nomeUnico = Guid.NewGuid().ToString() + extensao;
            var caminhoFicheiro = Path.Combine(pastaUploads, nomeUnico);

            using (var stream = new FileStream(caminhoFicheiro, FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            var url = $"/uploads/fotos/{nomeUnico}";
            return Ok(new { url });
        }
    }
}