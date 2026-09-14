using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Endpoints;

/// Nome do utilizador - o unico campo de perfil fora do que o
/// MapIdentityApi ja cobre (email, password, 2FA em /api/auth/manage).
/// Endpoint proprio porque o InfoResponse embutido do MapIdentityApi nao e
/// extensivel.
internal static class UserProfileEndpointHandlers
{
    public static async Task<IResult> GetMyProfile(ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var name = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(ct);

        return Results.Ok(new UserProfileResponse(name));
    }

    public static async Task<IResult> UpdateMyProfile(
        UpdateUserProfileRequest request, ClaimsPrincipal user, IdentityDbContext db, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (name is { Length: > 200 })
            return Results.BadRequest(new ErrorResponse("Nome inválido (máximo 200 caracteres)."));

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var zeloUser = await db.Users.FindAsync([userId], ct);
        if (zeloUser is null)
            return Results.Unauthorized();

        zeloUser.Name = string.IsNullOrEmpty(name) ? null : name;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new UserProfileResponse(zeloUser.Name));
    }
}
