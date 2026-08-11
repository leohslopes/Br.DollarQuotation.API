using Br.DollarQuotation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Br.DollarQuotation.Repository.Configurations;

public sealed class PasswordResetTokenConfiguration
    : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable(
            "password_reset_tokens"
        );

        ConfigurePrimaryKey(
            builder
        );

        ConfigureUser(
            builder
        );

        ConfigureToken(
            builder
        );

        ConfigureDates(
            builder
        );

        ConfigureIndexes(
            builder
        );
    }

    private static void ConfigurePrimaryKey(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(
            token => token.Id
        );

        builder.Property(
                token => token.Id
            )
            .HasColumnName(
                "id"
            )
            .ValueGeneratedNever();
    }

    private static void ConfigureUser(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.Property(
                token => token.UserId
            )
            .HasColumnName(
                "user_id"
            )
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(
                token => token.UserId
            )
            .OnDelete(
                DeleteBehavior.Cascade
            );
    }

    private static void ConfigureToken(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.Property(
                token => token.TokenHash
            )
            .HasColumnName(
                "token_hash"
            )
            .HasMaxLength(
                256
            )
            .IsRequired();
    }

    private static void ConfigureDates(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.Property(
                token => token.ExpiresAt
            )
            .HasColumnName(
                "expires_at"
            )
            .HasColumnType(
                "timestamp with time zone"
            )
            .IsRequired();

        builder.Property(
                token => token.UsedAt
            )
            .HasColumnName(
                "used_at"
            )
            .HasColumnType(
                "timestamp with time zone"
            );

        builder.Property(
                token => token.CreatedAt
            )
            .HasColumnName(
                "created_at"
            )
            .HasColumnType(
                "timestamp with time zone"
            )
            .IsRequired();
    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasIndex(
                token => token.TokenHash
            )
            .IsUnique()
            .HasDatabaseName(
                "ux_password_reset_tokens_token_hash"
            );

        builder.HasIndex(
                token => token.UserId
            )
            .HasDatabaseName(
                "ix_password_reset_tokens_user_id"
            );

        builder.HasIndex(
                token => new
                {
                    token.UserId,
                    token.ExpiresAt,
                    token.UsedAt
                }
            )
            .HasDatabaseName(
                "ix_password_reset_tokens_user_status"
            );
    }
}