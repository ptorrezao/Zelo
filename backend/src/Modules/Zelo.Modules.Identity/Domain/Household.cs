namespace Zelo.Modules.Identity.Domain;

internal sealed class Household
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    public List<HouseholdMember> Members { get; init; } = [];
}
