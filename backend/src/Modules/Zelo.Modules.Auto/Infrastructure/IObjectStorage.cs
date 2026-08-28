namespace Zelo.Modules.Auto.Infrastructure;

internal interface IObjectStorage
{
    /// URL pre-assinada para o cliente fazer upload direto (PUT), sem o
    /// ficheiro passar pela Api. objectKey deve ser unico (o chamador gera).
    (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType);
}
