using Microsoft.Extensions.Logging;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Consumers;

/// Gera automaticamente a foto de um veiculo assim que ele e criado - reage
/// ao AssetCreated ja publicado por CreateVehicleEntityAsync (ver
/// VehicleEvents.Created), nao existe (nem deve existir) um VehicleCreated
/// proprio so para isto. Nao ha sweep/retry nesta versao: se a geracao
/// falhar, o veiculo fica sem foto e mostra o placeholder de sempre - ver
/// plans/vehicle-image-generation.md, secao "Fora de ambito".
internal sealed class VehiclePhotoHandler(
    AutoDbContext db, IVehicleImageGenerator generator, IObjectStorage storage, ILogger<VehiclePhotoHandler> logger)
    : IEventHandler<AssetCreated>
{
    public async Task HandleAsync(AssetCreated @event, CancellationToken ct)
    {
        if (@event.Module != "auto" || @event.AssetType != "vehicle")
            return; // outro modulo (ex. Inventory) publicou o seu proprio AssetCreated

        var vehicle = await db.Vehicles.FindAsync([@event.AssetId], ct);
        if (vehicle is null || vehicle.PhotoObjectKey is not null)
            return; // apagado entretanto, ou (nao deveria acontecer) ja tem foto

        try
        {
            var png = await generator.GenerateVehiclePhotoAsync(vehicle, ct);
            var objectKey = $"vehicles/{vehicle.Id}/photo.png";
            await storage.UploadAsync(objectKey, png, "image/png", ct);

            vehicle.PhotoObjectKey = objectKey;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Falhar aqui nunca deve impedir o resto do fluxo de criacao do
            // veiculo, que ja aconteceu antes deste handler correr.
            logger.LogWarning(ex, "Falha a gerar foto para o veiculo {VehicleId}", vehicle.Id);
        }
    }
}
