using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Zelo.Messaging;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;
using Zelo.Modules.Auto.Infrastructure.VehicleCatalog;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Endpoints;

/// Tools MCP do Auto - o mesmo CRUD dos endpoints REST (AutoEndpointHandlers),
/// exposto a agentes LLM via Model Context Protocol. Montado em /mcp/auto
/// (ver AutoModule.MapAutoMcpEndpoints), atras da mesma autenticacao e feature
/// flag que o resto do modulo - ver docs/modules/module-contract.md, seccao 5.
///
/// Import fica de fora de proposito: exigiria o agente manusear credenciais
/// de outro utilizador (ver AutoEndpointHandlers.PreviewImport), risco
/// desproporcionado para o valor que traz aqui.
///
/// householdId e sempre o primeiro argumento explicito - o transporte MCP
/// nao tem query string, ao contrario do REST (ver nota em AutoEndpoints).
/// Cada tool confirma a membership antes de tocar em dados, e as operacoes
/// por id confirmam tambem que o recurso pertence a esse household -
/// RequireHouseholdMembership so cobre isto nos endpoints REST que recebem
/// householdId; aqui, por ser superficie nova, fecha-se o gap em vez de o
/// repetir. ListHouseholds existe para o agente conseguir descobrir esse
/// householdId por si so, sem o utilizador ter de o copiar a mao do
/// browser - sem isto nenhuma outra tool era utilizavel.
/// Nao e "static" apesar de so ter metodos estaticos - WithTools&lt;T&gt;()
/// usa T como argumento de tipo generico, e o C# nao permite tipos static
/// nessa posicao.
[McpServerToolType]
internal sealed class AutoMcpTools
{
    /// McpJsonUtilities.DefaultOptions serializa enums como o seu valor
    /// numerico - inutilizavel para um agente LLM adivinhar (0, 1, 2...).
    /// A Api HTTP já usa JsonStringEnumConverter (ver Program.cs); aqui é
    /// preciso o mesmo, mas em separado - o MCP SDK não partilha as
    /// opções configuradas em ConfigureHttpJsonOptions.
    public static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static Guid RequireUserId(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User
            ?? throw new McpException("Sem contexto de utilizador autenticado.");

        return Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new McpException("Sem contexto de utilizador autenticado.");
    }

    private static async Task EnsureMemberAsync(
        Guid householdId, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        var userId = RequireUserId(httpContextAccessor);
        if (!await membership.IsMemberAsync(userId, householdId, ct))
            throw new McpException("Sem acesso a este household.");
    }

    private static async Task<Vehicle> EnsureVehicleInHouseholdAsync(
        Guid vehicleId, Guid householdId, AutoDbContext db, CancellationToken ct) =>
        await db.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.HouseholdId == householdId, ct)
            ?? throw new McpException("Veículo não encontrado neste household.");

    /// Maintenances/Documents nao tem HouseholdId proprio - so o veiculo a
    /// que pertencem. Devolve o VehicleId para quem chama poder reusa-lo.
    private static async Task<Guid> EnsureMaintenanceInHouseholdAsync(
        Guid maintenanceId, Guid householdId, AutoDbContext db, CancellationToken ct)
    {
        var vehicleId = await db.Maintenances
            .Where(m => m.Id == maintenanceId)
            .Select(m => m.VehicleId)
            .FirstOrDefaultAsync(ct);

        if (vehicleId == Guid.Empty || !await db.Vehicles.AnyAsync(v => v.Id == vehicleId && v.HouseholdId == householdId, ct))
            throw new McpException("Manutenção não encontrada neste household.");

        return vehicleId;
    }

    private static async Task<Guid> EnsureDocumentInHouseholdAsync(
        Guid documentId, Guid householdId, AutoDbContext db, CancellationToken ct)
    {
        var vehicleId = await db.Documents
            .Where(d => d.Id == documentId)
            .Select(d => d.VehicleId)
            .FirstOrDefaultAsync(ct);

        if (vehicleId == Guid.Empty || !await db.Vehicles.AnyAsync(v => v.Id == vehicleId && v.HouseholdId == householdId, ct))
            throw new McpException("Documento não encontrado neste household.");

        return vehicleId;
    }

    // ---- Households ----

    [McpServerTool(Name = "list_households", ReadOnly = true)]
    [Description("Lista os households do utilizador autenticado, com o respetivo householdId - chama isto primeiro se não souberes o householdId a usar nas outras tools.")]
    public static Task<IReadOnlyList<HouseholdSummary>> ListHouseholds(
        IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct) =>
        membership.GetMyHouseholdsAsync(RequireUserId(httpContextAccessor), ct);

    [McpServerTool(Name = "create_household", Destructive = false)]
    [Description("Cria um novo household, com o utilizador autenticado como Owner.")]
    public static async Task<HouseholdSummary> CreateHousehold(
        [Description("Nome do household, ex. \"Casa de férias\".")] string name,
        IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        try
        {
            return await membership.CreateHouseholdAsync(RequireUserId(httpContextAccessor), name, ct);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "rename_household", Destructive = false, Idempotent = true)]
    [Description("Renomeia um household existente. Só o Owner do household o pode fazer.")]
    public static async Task<HouseholdSummary> RenameHousehold(
        [Description("Id do household a renomear.")] Guid householdId,
        [Description("Novo nome.")] string name,
        IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        try
        {
            return await membership.RenameHouseholdAsync(RequireUserId(httpContextAccessor), householdId, name, ct)
                ?? throw new McpException("Household não encontrado.");
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    // ---- Veículos ----

    [McpServerTool(Name = "list_vehicles", ReadOnly = true)]
    [Description("Lista os veículos registados num household.")]
    public static async Task<List<VehicleResponse>> ListVehicles(
        [Description("Id do household cujos veículos se quer listar.")] Guid householdId,
        AutoDbContext db, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        return await AutoEndpointHandlers.GetVehicles(householdId, db, storage, ct);
    }

    [McpServerTool(Name = "get_vehicle_catalog", ReadOnly = true)]
    [Description("Devolve o catálogo estático de marcas e modelos de veículos, por categoria (Ligeiros/Motociclos) - referência, não dados de household.")]
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>> GetVehicleCatalog() => VehicleCatalogLoader.Get();

    [McpServerTool(Name = "get_vehicle", ReadOnly = true)]
    [Description("Obtém os detalhes de um veículo pelo id.")]
    public static async Task<VehicleResponse> GetVehicle(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        AutoDbContext db, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        var vehicle = await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);
        return VehicleResponse.From(vehicle, storage);
    }

    [McpServerTool(Name = "create_vehicle", Destructive = false)]
    [Description("Regista um novo veículo num household.")]
    public static async Task<VehicleResponse> CreateVehicle(
        [Description("Id do household a que o veículo vai pertencer.")] Guid householdId,
        [Description("Categoria: Ligeiros ou Motociclos.")] VehicleCategory category,
        [Description("Marca, ex. \"Toyota\".")] string brand,
        [Description("Modelo, ex. \"Corolla\".")] string model,
        [Description("Matrícula.")] string plate,
        [Description("Número de identificação do veículo (VIN/chassis).")] string vin,
        [Description("Cor.")] string? color,
        [Description("Nome do condutor habitual.")] string? driver,
        [Description("Quilometragem atual.")] int odometer,
        [Description("Data de matrícula, formato AAAA-MM-DD.")] DateOnly registered,
        [Description("Data da próxima inspeção periódica, se conhecida.")] DateOnly? nextInspection,
        [Description("Seguradora.")] string? insurer,
        [Description("Número da apólice de seguro.")] string? insurancePolicyNumber,
        [Description("Início do período de seguro em vigor.")] DateOnly? insurancePeriodStart,
        [Description("Fim do período de seguro em vigor.")] DateOnly? insurancePeriodEnd,
        [Description("Prémio de seguro anual.")] decimal? insurancePremium,
        [Description("Data limite de pagamento do IUC.")] DateOnly? iucDueDate,
        AutoDbContext db, IEventPublisher events, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);

        var request = new VehicleUpsertRequest(
            category, brand, model, plate, vin, color, driver, odometer, registered, nextInspection,
            insurer, insurancePolicyNumber, insurancePeriodStart, insurancePeriodEnd, insurancePremium, iucDueDate);
        try
        {
            var vehicle = await AutoEndpointHandlers.CreateVehicleEntityAsync(householdId, request, db, events, ct);
            return VehicleResponse.From(vehicle, storage);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "update_vehicle", Destructive = false, Idempotent = true)]
    [Description("Atualiza os dados de um veículo existente. Substitui todos os campos - envie os valores atuais nos campos que não quer mudar.")]
    public static async Task<VehicleResponse> UpdateVehicle(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo a atualizar.")] Guid vehicleId,
        [Description("Categoria: Ligeiros ou Motociclos.")] VehicleCategory category,
        [Description("Marca.")] string brand,
        [Description("Modelo.")] string model,
        [Description("Matrícula.")] string plate,
        [Description("Número de identificação do veículo (VIN/chassis).")] string vin,
        [Description("Cor.")] string? color,
        [Description("Nome do condutor habitual.")] string? driver,
        [Description("Quilometragem atual.")] int odometer,
        [Description("Data de matrícula, formato AAAA-MM-DD.")] DateOnly registered,
        [Description("Data da próxima inspeção periódica, se conhecida.")] DateOnly? nextInspection,
        [Description("Seguradora.")] string? insurer,
        [Description("Número da apólice de seguro.")] string? insurancePolicyNumber,
        [Description("Início do período de seguro em vigor.")] DateOnly? insurancePeriodStart,
        [Description("Fim do período de seguro em vigor.")] DateOnly? insurancePeriodEnd,
        [Description("Prémio de seguro anual.")] decimal? insurancePremium,
        [Description("Data limite de pagamento do IUC.")] DateOnly? iucDueDate,
        AutoDbContext db, IEventPublisher events, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);

        var request = new VehicleUpsertRequest(
            category, brand, model, plate, vin, color, driver, odometer, registered, nextInspection,
            insurer, insurancePolicyNumber, insurancePeriodStart, insurancePeriodEnd, insurancePremium, iucDueDate);
        try
        {
            var vehicle = await AutoEndpointHandlers.UpdateVehicleEntityAsync(vehicleId, request, db, events, ct)
                ?? throw new McpException("Veículo não encontrado neste household.");
            return VehicleResponse.From(vehicle, storage);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "archive_vehicle", Idempotent = true)]
    [Description("Marca um veículo como Vendido ou Abatido. Não apaga o histórico de manutenções/documentos - só deixa de aparecer como ativo.")]
    public static async Task<VehicleResponse> ArchiveVehicle(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo a arquivar.")] Guid vehicleId,
        [Description("Novo estado: Vendido ou Abatido.")] VehicleStatus status,
        AutoDbContext db, IEventPublisher events, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);

        var vehicle = await AutoEndpointHandlers.ArchiveVehicleEntityAsync(vehicleId, status, db, events, ct)
            ?? throw new McpException("Veículo não encontrado neste household.");
        return VehicleResponse.From(vehicle, storage);
    }

    [McpServerTool(Name = "get_vehicle_stats", ReadOnly = true)]
    [Description("Estatísticas do último mês para um veículo: kms percorridos, custo e número de manutenções.")]
    public static async Task<VehicleStatsResponse> GetVehicleStats(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);
        return await AutoEndpointHandlers.GetStatsAsync(vehicleId, db, ct);
    }

    // ---- Manutenções ----

    [McpServerTool(Name = "list_maintenances", ReadOnly = true)]
    [Description("Lista as manutenções (revisões, reparações, inspeções) de um veículo.")]
    public static async Task<List<MaintenanceResponse>> ListMaintenances(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);
        return await AutoEndpointHandlers.GetMaintenances(vehicleId, db, ct);
    }

    [McpServerTool(Name = "create_maintenance", Destructive = false)]
    [Description("Regista uma manutenção (revisão, reparação ou inspeção) de um veículo.")]
    public static async Task<MaintenanceResponse> CreateMaintenance(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        [Description("Data da manutenção, formato AAAA-MM-DD.")] DateOnly date,
        [Description("Quilometragem no momento da manutenção.")] int odometer,
        [Description("Nome do stand/oficina.")] string workshop,
        [Description("Descrição do serviço realizado.")] string description,
        [Description("Tipo: Preventiva, Corretiva ou Inspecao.")] MaintenanceType type,
        [Description("Custo total.")] decimal cost,
        [Description("Número da fatura, se houver.")] string? invoiceNumber,
        [Description("Data da fatura, se houver.")] DateOnly? invoiceDate,
        [Description("Peças/serviços incluídos, cada um com descrição, preço e número de série opcional.")] IReadOnlyList<MaintenanceItemRequest>? items,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);

        var request = new MaintenanceUpsertRequest(date, odometer, workshop, description, type, cost, invoiceNumber, invoiceDate, items);
        var maintenance = await AutoEndpointHandlers.CreateMaintenanceEntityAsync(vehicleId, request, db, ct)
            ?? throw new McpException("Veículo não encontrado neste household.");
        return MaintenanceResponse.From(maintenance);
    }

    [McpServerTool(Name = "get_maintenance", ReadOnly = true)]
    [Description("Obtém os detalhes de uma manutenção pelo id.")]
    public static async Task<MaintenanceResponse> GetMaintenance(
        [Description("Id do household a que a manutenção deve pertencer (através do veículo).")] Guid householdId,
        [Description("Id da manutenção.")] Guid maintenanceId,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureMaintenanceInHouseholdAsync(maintenanceId, householdId, db, ct);

        var maintenance = await db.Maintenances.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == maintenanceId, ct)
            ?? throw new McpException("Manutenção não encontrada neste household.");
        return MaintenanceResponse.From(maintenance);
    }

    [McpServerTool(Name = "update_maintenance", Destructive = false, Idempotent = true)]
    [Description("Atualiza uma manutenção existente. Substitui todos os campos - envie os valores atuais nos campos que não quer mudar.")]
    public static async Task<MaintenanceResponse> UpdateMaintenance(
        [Description("Id do household a que a manutenção deve pertencer (através do veículo).")] Guid householdId,
        [Description("Id da manutenção a atualizar.")] Guid maintenanceId,
        [Description("Data da manutenção, formato AAAA-MM-DD.")] DateOnly date,
        [Description("Quilometragem no momento da manutenção.")] int odometer,
        [Description("Nome do stand/oficina.")] string workshop,
        [Description("Descrição do serviço realizado.")] string description,
        [Description("Tipo: Preventiva, Corretiva ou Inspecao.")] MaintenanceType type,
        [Description("Custo total.")] decimal cost,
        [Description("Número da fatura, se houver.")] string? invoiceNumber,
        [Description("Data da fatura, se houver.")] DateOnly? invoiceDate,
        [Description("Peças/serviços incluídos, cada um com descrição, preço e número de série opcional.")] IReadOnlyList<MaintenanceItemRequest>? items,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureMaintenanceInHouseholdAsync(maintenanceId, householdId, db, ct);

        var request = new MaintenanceUpsertRequest(date, odometer, workshop, description, type, cost, invoiceNumber, invoiceDate, items);
        var maintenance = await AutoEndpointHandlers.UpdateMaintenanceEntityAsync(maintenanceId, request, db, ct)
            ?? throw new McpException("Manutenção não encontrada neste household.");
        return MaintenanceResponse.From(maintenance);
    }

    [McpServerTool(Name = "delete_maintenance", Idempotent = true)]
    [Description("Elimina permanentemente um registo de manutenção.")]
    public static async Task DeleteMaintenance(
        [Description("Id do household a que a manutenção deve pertencer (através do veículo).")] Guid householdId,
        [Description("Id da manutenção a eliminar.")] Guid maintenanceId,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureMaintenanceInHouseholdAsync(maintenanceId, householdId, db, ct);

        if (!await AutoEndpointHandlers.DeleteMaintenanceEntityAsync(maintenanceId, db, ct))
            throw new McpException("Manutenção não encontrada neste household.");
    }

    // ---- Documentos ----

    [McpServerTool(Name = "list_documents", ReadOnly = true)]
    [Description("Lista os documentos (apólices, faturas, registos) de um veículo, opcionalmente filtrados por categoria.")]
    public static async Task<List<DocumentResponse>> ListDocuments(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        [Description("Filtra por categoria: Seguro, Manutencao, Inspecao, Registo ou Fatura. Omitir para listar todas.")] DocumentCategory? category,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);
        return await AutoEndpointHandlers.GetDocuments(vehicleId, category, db, ct);
    }

    [McpServerTool(Name = "create_document_upload_url", ReadOnly = true)]
    [Description("Gera um URL pré-assinado para carregar o ficheiro de um documento antes de o registar com create_document. Só é útil se o cliente MCP conseguir fazer o upload binário para esse URL.")]
    public static async Task<UploadUrlResponse> CreateDocumentUploadUrl(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        [Description("Nome do ficheiro.")] string fileName,
        [Description("Content-Type do ficheiro, ex. \"application/pdf\".")] string contentType,
        AutoDbContext db, IObjectStorage storage, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);

        var request = new UploadUrlRequest(fileName, contentType);
        return AutoEndpointHandlers.CreateUploadUrlResponse(vehicleId, request, storage);
    }

    [McpServerTool(Name = "create_document", Destructive = false)]
    [Description("Regista um documento de um veículo, depois de o ficheiro já ter sido carregado para o objectKey devolvido por create_document_upload_url.")]
    public static async Task<DocumentResponse> CreateDocument(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do veículo.")] Guid vehicleId,
        [Description("Object key devolvido por create_document_upload_url.")] string objectKey,
        [Description("Nome do documento.")] string name,
        [Description("Categoria: Seguro, Manutencao, Inspecao, Registo ou Fatura.")] DocumentCategory category,
        [Description("Tipo de ficheiro: Pdf ou Imagem.")] DocumentType type,
        [Description("Data do documento, formato AAAA-MM-DD.")] DateOnly date,
        [Description("Tamanho do ficheiro em bytes.")] long sizeBytes,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureVehicleInHouseholdAsync(vehicleId, householdId, db, ct);

        var request = new DocumentCreateRequest(objectKey, name, category, type, date, sizeBytes);
        var document = await AutoEndpointHandlers.CreateDocumentEntityAsync(vehicleId, request, db, ct)
            ?? throw new McpException("Veículo não encontrado neste household.");
        return DocumentResponse.From(document);
    }

    [McpServerTool(Name = "delete_document", Idempotent = true)]
    [Description("Elimina permanentemente um documento (não elimina o ficheiro no armazenamento, só o registo).")]
    public static async Task DeleteDocument(
        [Description("Id do household a que o veículo deve pertencer.")] Guid householdId,
        [Description("Id do documento a eliminar.")] Guid documentId,
        AutoDbContext db, IHouseholdMembershipChecker membership, IHttpContextAccessor httpContextAccessor, CancellationToken ct)
    {
        await EnsureMemberAsync(householdId, membership, httpContextAccessor, ct);
        await EnsureDocumentInHouseholdAsync(documentId, householdId, db, ct);

        if (!await AutoEndpointHandlers.DeleteDocumentEntityAsync(documentId, db, ct))
            throw new McpException("Documento não encontrado neste household.");
    }
}
