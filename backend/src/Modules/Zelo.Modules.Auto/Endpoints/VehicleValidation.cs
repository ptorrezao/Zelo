namespace Zelo.Modules.Auto.Endpoints;

/// Validacao partilhada entre CreateVehicleEntityAsync e
/// UpdateVehicleEntityAsync (por isso cobre REST, MCP e ConfirmImport, que
/// chamam os dois) - lanca ArgumentException na primeira violacao, quem
/// chama decide como traduzir isso (BadRequest no REST, McpException no
/// MCP, item "nao importado" no ConfirmImport), mesmo padrao que
/// HouseholdProvisioning no Identity.
internal static class VehicleValidation
{
    public static void Validate(VehicleUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Brand))
            throw new ArgumentException("Marca é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Modelo é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Plate))
            throw new ArgumentException("Matrícula é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Vin))
            throw new ArgumentException("VIN é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Color))
            throw new ArgumentException("Cor é obrigatória.");
        if (request.Odometer < 0)
            throw new ArgumentException("Quilometragem não pode ser negativa.");
        if (request.Registered > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data de matrícula não pode ser no futuro.");
        if (request.InsurancePeriodStart is { } start && request.InsurancePeriodEnd is { } end && end < start)
            throw new ArgumentException("Fim do período de seguro não pode ser antes do início.");
        if (request.InsurancePremium is < 0)
            throw new ArgumentException("Prémio de seguro não pode ser negativo.");
    }
}
