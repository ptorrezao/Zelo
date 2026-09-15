using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure;

/// Abstrai o fornecedor de geracao de imagem (hoje OpenAI/gpt-image-1) -
/// VehiclePhotoHandler so depende desta interface, nunca do provider
/// concreto. Trocar ou acrescentar um segundo fornecedor (ex.: Gemini/Nano
/// Banana) e uma nova implementacao + um HttpClient tipado registado em
/// AutoModule.AddAutoModule, sem tocar no handler nem no prompt.
internal interface IVehicleImageGenerator
{
    Task<byte[]> GenerateVehiclePhotoAsync(Vehicle vehicle, CancellationToken ct = default);
}
