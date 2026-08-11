using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Br.DollarQuotation.Repository.Configurations;

public sealed class QuotationAlertConfiguration
    : IEntityTypeConfiguration<QuotationAlert>
{
    public void Configure(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.ToTable("quotation_alerts");

        ConfigurePrimaryKey(builder);
        ConfigureUser(builder);
        ConfigureCurrencyPair(builder);
        ConfigureCondition(builder);
        ConfigureTargetPrice(builder);
        ConfigureStatus(builder);
        ConfigureDates(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigurePrimaryKey(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.HasKey(alert => alert.Id);

        builder.Property(alert => alert.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
    }

    private static void ConfigureUser(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.Property(alert => alert.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(alert => alert.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureCurrencyPair(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        var converter = new ValueConverter<CurrencyPair, string>(
            pair => pair.ToCode(),
            code => CurrencyPair.FromCode(code)
        );

        var comparer = new ValueComparer<CurrencyPair>(
            (left, right) => left == right,
            pair => pair.GetHashCode(),
            pair => CurrencyPair.Create(
                pair.BaseCurrency,
                pair.QuoteCurrency
            )
        );

        builder.Property(alert => alert.CurrencyPair)
            .HasColumnName("currency_pair")
            .HasMaxLength(20)
            .HasConversion(converter)
            .Metadata.SetValueComparer(comparer);

        builder.Property(alert => alert.CurrencyPair)
            .IsRequired();
    }

    private static void ConfigureCondition(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.Property(alert => alert.Condition)
            .HasColumnName("condition")
            .HasConversion<int>()
            .IsRequired();
    }

    private static void ConfigureTargetPrice(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.Property(alert => alert.TargetPrice)
            .HasColumnName("target_price")
            .HasPrecision(20, 8)
            .IsRequired();
    }

    private static void ConfigureStatus(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.Property(alert => alert.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }

    private static void ConfigureDates(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.Property(alert => alert.TriggeredAt)
            .HasColumnName("triggered_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(alert => alert.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(alert => alert.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");
    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<QuotationAlert> builder)
    {
        builder.HasIndex(alert => alert.UserId)
            .HasDatabaseName(
                "ix_quotation_alerts_user_id"
            );

        builder.HasIndex(alert => alert.CurrencyPair)
            .HasDatabaseName(
                "ix_quotation_alerts_currency_pair"
            );

        builder.HasIndex(alert => alert.IsActive)
            .HasDatabaseName(
                "ix_quotation_alerts_is_active"
            );

        builder.HasIndex(
                alert => new
                {
                    alert.UserId,
                    alert.CurrencyPair,
                    alert.IsActive
                })
            .HasDatabaseName(
                "ix_quotation_alerts_user_pair_active"
            );
    }
}