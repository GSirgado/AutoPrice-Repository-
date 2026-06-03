using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UploadController(IConfiguration config)
        {
            _config = config;
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

            // Ligar ao Azure Blob Storage
            var connectionString = _config["AzureStorage:ConnectionString"];
            var containerName = _config["AzureStorage:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Gerar nome único para o ficheiro
            var extensao = Path.GetExtension(ficheiro.FileName);
            var nomeUnico = Guid.NewGuid().ToString() + extensao;
            var blobClient = containerClient.GetBlobClient(nomeUnico);

            // Fazer upload para o Azure
            using var stream = ficheiro.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = ficheiro.ContentType
            });

            // Devolver o URL público da imagem
            var url = blobClient.Uri.ToString();
            return Ok(new { url });
        }
    }
}