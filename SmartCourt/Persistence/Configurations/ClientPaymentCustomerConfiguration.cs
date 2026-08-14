using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ClientPaymentCustomerConfiguration
    : IEntityTypeConfiguration<ClientPaymentCustomer>
{
    public void Configure(EntityTypeBuilder<ClientPaymentCustomer> builder)
    {
        builder.ToTable("ClientPaymentCustomers");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProviderCode)
            .IsRequired().IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.ProviderCustomerId)
            .IsRequired().IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion)
            .IsRowVersion().IsConcurrencyToken();
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(item => item.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.ClientUserId, item.ProviderCode })
            .IsUnique()
            .HasDatabaseName("UX_ClientPaymentCustomers_Client_Provider");
        builder.HasIndex(item => new { item.ProviderCode, item.ProviderCustomerId })
            .IsUnique()
            .HasDatabaseName("UX_ClientPaymentCustomers_ProviderCustomer");
    }
}
