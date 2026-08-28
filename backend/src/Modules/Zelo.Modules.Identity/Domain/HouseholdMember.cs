namespace Zelo.Modules.Identity.Domain;

internal enum HouseholdRole
{
    Owner,
    Member
}

internal sealed class HouseholdMember
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public Guid UserId { get; init; }
    public HouseholdRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; init; }

    public Household Household { get; init; } = null!;
}
