using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Br.DollarQuotation.Repository.Configurations;

public sealed class CurrencyQuotationConfiguration
    : IEntityTypeConfiguration<CurrencyQuotation>
{
    public void Configure(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        builder.ToTable("currency_quotations");

        ConfigurePrimaryKey(builder);
        ConfigureCurrencyPair(builder);
        ConfigurePrices(builder);
        ConfigureDates(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigurePrimaryKey(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        builder.HasKey(quotation => quotation.Id);

        builder.Property(quotation => quotation.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
    }

    private static void ConfigureCurrencyPair(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        var converter = new ValueConverter<CurrencyPair, string>(
            pair => pair.ToCode(),
            code => CurrencyPair.FromCode(code));

        var comparer = new ValueComparer<CurrencyPair>(
            (left, right) => left == right,
            pair => pair.GetHashCode(),
            pair => CurrencyPair.Create(
                pair.BaseCurrency,
                pair.QuoteCurrency));

        builder.Property(quotation => quotation.CurrencyPair)
            .HasColumnName("currency_pair")
            .HasMaxLength(20)
            .HasConversion(converter)
            .Metadata.SetValueComparer(comparer);

        builder.Property(quotation => quotation.CurrencyPair)
            .IsRequired();
    }

    private static void ConfigurePrices(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        builder.Property(quotation => quotation.BidPrice)
            .HasColumnName("bid_price")
            .HasPrecision(20, 8)
            .IsRequired();

        builder.Property(quotation => quotation.AskPrice)
            .HasColumnName("ask_price")
            .HasPrecision(20, 8)
            .IsRequired();

        builder.Property(quotation => quotation.HighPrice)
            .HasColumnName("high_price")
            .HasPrecision(20, 8)
            .IsRequired();

        builder.Property(quotation => quotation.LowPrice)
            .HasColumnName("low_price")
            .HasPrecision(20, 8)
            .IsRequired();

        builder.Property(quotation => quotation.Variation)
            .HasColumnName("variation")
            .HasPrecision(20, 8)
            .IsRequired();

        builder.Property(quotation => quotation.VariationPercentage)
            .HasColumnName("variation_percentage")
            .HasPrecision(20, 8)
            .IsRequired();
    }

    private static void ConfigureDates(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        builder.Property(quotation => quotation.QuotationDate)
            .HasColumnName("quotation_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(quotation => quotation.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<CurrencyQuotation> builder)
    {
        builder.HasIndex(quotation => quotation.CurrencyPair)
            .HasDatabaseName(
                "ix_currency_quotations_currency_pair");

        builder.HasIndex(quotation => quotation.QuotationDate)
            .HasDatabaseName(
                "ix_currency_quotations_quotation_date");

        builder.HasIndex(
                quotation => new
                {
                    quotation.CurrencyPair,
                    quotation.QuotationDate
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_currency_quotations_pair_date");
    }
}