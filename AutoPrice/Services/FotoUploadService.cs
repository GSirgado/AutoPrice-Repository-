using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AutoPrice.Services
{
    // Faz o upload de uma foto diretamente para o Azure Blob Storage.
    // Antes disto era feito através de um pedido HTTP ao UploadController do
    // AutoMarket; agora o AutoPrice fala diretamente com o Storage, tal como o
    // AutoMarket também faz, sem passar um pelo outro.
    public class FotoUploadService
    {
        private static readonly string[] TiposPermitidos = { "image/jpeg", "image/png", "image/webp" };
        private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5MB

        private readonly IConfiguration _config;

        public FotoUploadService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<(bool Sucesso, string? Url, string? Erro)> UploadAsync(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return (false, null, "Nenhum ficheiro enviado.");

            if (!TiposPermitidos.Contains(ficheiro.ContentType))
                return (false, null, "Tipo de ficheiro não permitido.");

            if (ficheiro.Length > TamanhoMaximoBytes)
                return (false, null, "Ficheiro demasiado grande. Máximo 5MB.");

            var connectionString = _config["AzureStorage:ConnectionString"];
            var containerName = _config["AzureStorage:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var extensao = Path.GetExtension(ficheiro.FileName);
            var nomeUnico = Guid.NewGuid().ToString() + extensao;
            var blobClient = containerClient.GetBlobClient(nomeUnico);

            using var stream = ficheiro.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = ficheiro.ContentType
            });

            return (true, blobClient.Uri.ToString(), null);
        }
    }
}
