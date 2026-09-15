namespace Zelo.Modules.Auto.Infrastructure;

internal interface IObjectStorage
{
    /// URL pre-assinada para o cliente fazer upload direto (PUT), sem o
    /// ficheiro passar pela Api. objectKey deve ser unico (o chamador gera).
    (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType);

    /// Upload feito pelo proprio servidor (ex.: Worker a gravar uma imagem
    /// gerada) - ao contrario de CreateUploadUrl, nao ha um browser do outro
    /// lado, por isso nao faz sentido uma URL pre-assinada aqui.
    Task UploadAsync(string objectKey, byte[] content, string contentType, CancellationToken ct = default);

    /// URL pre-assinada de leitura (GET), validade curta - usada para servir
    /// ficheiros privados (ex. foto do veiculo) sem expor o bucket como
    /// publico nem fazer a Api proxiar os bytes.
    Uri CreateReadUrl(string objectKey, TimeSpan validFor);
}
