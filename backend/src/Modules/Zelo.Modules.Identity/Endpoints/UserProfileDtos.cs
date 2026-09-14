namespace Zelo.Modules.Identity.Endpoints;

internal sealed record UserProfileResponse(string? Name);

internal sealed record UpdateUserProfileRequest(string? Name);
