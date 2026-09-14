using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Endpoints;

internal sealed record HouseholdResponse(Guid Id, string Name, HouseholdRole Role, bool IsDefault);

internal sealed record HouseholdUpdateRequest(string Name);

/// Corpo dos 400 dos endpoints deste modulo (households e perfil) - tipo
/// proprio em vez de "new { error = ... }" anonimo para o schema OpenAPI
/// (Produces<T>()) ter algo concreto a apontar.
internal sealed record ErrorResponse(string Error);
