using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Endpoints;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Tests;

public class UserProfileEndpointHandlersTests
{
    private static IdentityDbContext NewDb() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ClaimsPrincipal PrincipalFor(Guid userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));

    private static async Task<ZeloUser> AddUserAsync(IdentityDbContext db, Guid userId, string? name = null)
    {
        var user = new ZeloUser { Id = userId, UserName = "user@teste.com", Email = "user@teste.com", Name = name };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetMyProfile_UtilizadorSemNomeDefinido_DevolveNomeNulo()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await AddUserAsync(db, userId);

        var result = await UserProfileEndpointHandlers.GetMyProfile(PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserProfileResponse>>(result);
        Assert.Null(ok.Value!.Name);
    }

    [Fact]
    public async Task GetMyProfile_UtilizadorComNome_DevolveNome()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await AddUserAsync(db, userId, "Pedro Torrezão");

        var result = await UserProfileEndpointHandlers.GetMyProfile(PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserProfileResponse>>(result);
        Assert.Equal("Pedro Torrezão", ok.Value!.Name);
    }

    [Fact]
    public async Task UpdateMyProfile_DefineNome()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await AddUserAsync(db, userId);

        var result = await UserProfileEndpointHandlers.UpdateMyProfile(
            new UpdateUserProfileRequest("Pedro Torrezão"), PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserProfileResponse>>(result);
        Assert.Equal("Pedro Torrezão", ok.Value!.Name);
        Assert.Equal("Pedro Torrezão", (await db.Users.FindAsync(userId))!.Name);
    }

    [Fact]
    public async Task UpdateMyProfile_NomeVazio_LimpaONome()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await AddUserAsync(db, userId, "Nome antigo");

        var result = await UserProfileEndpointHandlers.UpdateMyProfile(
            new UpdateUserProfileRequest("   "), PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserProfileResponse>>(result);
        Assert.Null(ok.Value!.Name);
        Assert.Null((await db.Users.FindAsync(userId))!.Name);
    }

    [Fact]
    public async Task UpdateMyProfile_NomeDemasiadoLongo_DevolveBadRequest()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await AddUserAsync(db, userId);

        var result = await UserProfileEndpointHandlers.UpdateMyProfile(
            new UpdateUserProfileRequest(new string('a', 201)), PrincipalFor(userId), db, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_SemClaimValida_DevolveUnauthorized()
    {
        await using var db = NewDb();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await UserProfileEndpointHandlers.GetMyProfile(principal, db, CancellationToken.None);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
