namespace Zelo.Modules.Auto.Infrastructure;

internal sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Endpoint { get; set; } = "http://localhost:3900";

    /// Host alternativo usado so para pedidos que o proprio servidor faz
    /// diretamente ao Garage (ex.: UploadAsync, chamado pelo Worker) - por
    /// omissao igual a Endpoint. Em dev local, Endpoint tem de ser
    /// "localhost" (o browser resolve as URLs pre-assinadas fora de
    /// Docker), mas dentro dos containers "localhost" e o proprio
    /// container, nao o Garage - InternalEndpoint aponta para o nome do
    /// servico Docker (ver docker-compose.yml). Em producao normalmente
    /// coincide com Endpoint, por isso fica opcional.
    public string? InternalEndpoint { get; set; }
    public string Region { get; set; } = "garage";
    public string Bucket { get; set; } = "zelo-documents";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}
