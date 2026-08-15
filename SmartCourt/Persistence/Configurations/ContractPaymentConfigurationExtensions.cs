using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartCourt.Persistence.Configurations;

internal static class ContractPaymentConfigurationExtensions
{
    internal static PropertyBuilder<decimal> Money(
        this PropertyBuilder<decimal> property)
    {
        return property
            .HasPrecision(18, 2)
            .HasColumnType("decimal(18,2)");
    }

    internal static PropertyBuilder<decimal?> Money(
        this PropertyBuilder<decimal?> property)
    {
        return property
            .HasPrecision(18, 2)
            .HasColumnType("decimal(18,2)");
    }

    internal static PropertyBuilder<DateTimeOffset> Utc(
        this PropertyBuilder<DateTimeOffset> property)
    {
        return property.HasColumnType("datetimeoffset");
    }

    internal static PropertyBuilder<DateTimeOffset?> NullableUtc(
        this PropertyBuilder<DateTimeOffset?> property)
    {
        return property.HasColumnType("datetimeoffset");
    }

    internal static PropertyBuilder<string> Unicode(
        this PropertyBuilder<string> property,
        int maxLength)
    {
        return property
            .IsUnicode()
            .HasMaxLength(maxLength);
    }

    internal static PropertyBuilder<string?> NullableUnicode(
        this PropertyBuilder<string?> property,
        int maxLength)
    {
        return property
            .IsUnicode()
            .HasMaxLength(maxLength);
    }
}
