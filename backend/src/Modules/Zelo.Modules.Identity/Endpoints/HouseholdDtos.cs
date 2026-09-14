using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Endpoints;

internal sealed record HouseholdResponse(Guid Id, string Name, HouseholdRole Role);

internal sealed record HouseholdUpdateRequest(string Name);
