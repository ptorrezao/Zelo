using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Infrastructure.Configurations;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Name).HasMaxLength(200).IsRequired();
        builder.Property(k => k.KeyHash).HasMaxLength(64).IsRequired();
        builder.Property(k => k.DisplayPrefix).HasMaxLength(16).IsRequired();

        // E por aqui que ApiKeyAuthenticationHandler procura em cada
        // pedido - tem de ser rapido mesmo com muitas chaves na tabela.
        builder.HasIndex(k => k.KeyHash).IsUnique();
        builder.HasIndex(k => k.UserId);
    }
}
