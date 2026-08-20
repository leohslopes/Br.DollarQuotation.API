using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Br.DollarQuotation.Repository.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        ConfigurePrimaryKey(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigurePrimaryKey(
        EntityTypeBuilder<User> builder)
    {
        builder.HasKey(
            user => user.Id);

        builder.Property(
                user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
    }

    private static void ConfigureProperties(
        EntityTypeBuilder<User> builder)
    {
        builder.Property(
                user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(
                user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .IsRequired();

        builder.Property(
                user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(
                user => user.PhotoBase64)
            .HasColumnName("photo_base64")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(
                user => user.PhotoContentType)
            .HasColumnName("photo_content_type")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(user => user.Role)
                .HasColumnName("role")
                .HasConversion<int>()
                .IsRequired();

        builder.Property(
                user => user.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(
                user => user.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(
                user => user.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);
    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(
                user => user.Email)
            .IsUnique()
            .HasDatabaseName(
                "ux_users_email");
    }
}