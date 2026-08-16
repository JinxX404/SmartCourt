using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Consultations.Domain.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class LawyerConsultationSettingsConfiguration
    : IEntityTypeConfiguration<LawyerConsultationSettings>
{
    public void Configure(EntityTypeBuilder<LawyerConsultationSettings> builder)
    {
        builder.ToTable("LawyerConsultationSettings");
        builder.HasKey(item => item.LawyerId);
        builder.Property(item => item.TimeZoneId).IsRequired().IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne<LawyerProfile>().WithMany().HasForeignKey(item => item.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasCheckConstraint("CK_LawyerConsultationSettings_Notice", "[MinimumBookingNoticeHours] BETWEEN 0 AND 168");
        builder.HasCheckConstraint("CK_LawyerConsultationSettings_Advance", "[MaximumAdvanceBookingDays] BETWEEN 1 AND 365");
        builder.HasCheckConstraint("CK_LawyerConsultationSettings_Buffer", "[BufferMinutes] BETWEEN 0 AND 120");
    }
}

public sealed class ConsultationOfferingConfiguration
    : IEntityTypeConfiguration<ConsultationOffering>
{
    public void Configure(EntityTypeBuilder<ConsultationOffering> builder)
    {
        builder.ToTable("ConsultationOfferings");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Mode).HasConversion<byte>();
        builder.Property(item => item.Specialization).HasConversion<byte>();
        builder.Property(item => item.Title).IsRequired().Unicode(120);
        builder.Property(item => item.Description).IsRequired().Unicode(2_000);
        builder.Property(item => item.Price).Money();
        builder.Property(item => item.Currency).IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.OfficeLocation).NullableUnicode(500);
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne<LawyerProfile>().WithMany().HasForeignKey(item => item.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.LawyerId, item.IsActive });
        builder.HasIndex(item => new { item.Mode, item.Specialization, item.IsActive });
        builder.HasCheckConstraint("CK_ConsultationOfferings_Duration", "[DurationMinutes] BETWEEN 15 AND 240");
        builder.HasCheckConstraint("CK_ConsultationOfferings_Price", "[Price] > 0 AND [Price] <= 100000");
        builder.HasCheckConstraint("CK_ConsultationOfferings_Currency", "[Currency] = 'EGP'");
    }
}

public sealed class ConsultationOfferingInclusionConfiguration
    : IEntityTypeConfiguration<ConsultationOfferingInclusion>
{
    public void Configure(EntityTypeBuilder<ConsultationOfferingInclusion> builder)
    {
        builder.ToTable("ConsultationOfferingInclusions");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Text).IsRequired().Unicode(200);
        builder.HasOne(item => item.Offering).WithMany(item => item.Inclusions)
            .HasForeignKey(item => item.OfferingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new { item.OfferingId, item.SortOrder }).IsUnique();
    }
}

public sealed class ConsultationAvailabilitySlotConfiguration
    : IEntityTypeConfiguration<ConsultationAvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<ConsultationAvailabilitySlot> builder)
    {
        builder.ToTable("ConsultationAvailabilitySlots");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasConversion<byte>();
        builder.Property(item => item.StartAtUtc).Utc();
        builder.Property(item => item.EndAtUtc).Utc();
        builder.Property(item => item.ReservedUntilUtc).NullableUtc();
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne(item => item.Offering).WithMany().HasForeignKey(item => item.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LawyerProfile>().WithMany().HasForeignKey(item => item.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.OfferingId, item.StartAtUtc })
            .IsUnique().HasFilter("[Status] <> 4");
        builder.HasIndex(item => new { item.LawyerId, item.StartAtUtc, item.EndAtUtc });
        builder.HasCheckConstraint("CK_ConsultationSlots_TimeRange", "[EndAtUtc] > [StartAtUtc]");
    }
}

public sealed class ConsultationBookingConfiguration
    : IEntityTypeConfiguration<ConsultationBooking>
{
    public void Configure(EntityTypeBuilder<ConsultationBooking> builder)
    {
        builder.ToTable("ConsultationBookings");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Mode).HasConversion<byte>();
        builder.Property(item => item.Specialization).HasConversion<byte>();
        builder.Property(item => item.Status).HasConversion<byte>();
        builder.Property(item => item.OfferingTitle).IsRequired().Unicode(120);
        builder.Property(item => item.OfferingDescription).IsRequired().Unicode(2_000);
        builder.Property(item => item.InclusionsJson).IsRequired().Unicode(3_000);
        builder.Property(item => item.Subject).IsRequired().Unicode(150);
        builder.Property(item => item.MatterSummary).IsRequired().Unicode(3_000);
        builder.Property(item => item.OfficeLocation).NullableUnicode(500);
        builder.Property(item => item.MeetingUrl).IsUnicode(false).HasMaxLength(1_000);
        builder.Property(item => item.CancellationReason).NullableUnicode(1_000);
        builder.Property(item => item.DisputeReason).NullableUnicode(2_000);
        builder.Property(item => item.GrossAmount).Money();
        builder.Property(item => item.PlatformFeeAmount).Money();
        builder.Property(item => item.LawyerNetAmount).Money();
        builder.Property(item => item.Currency).IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.StartAtUtc).Utc();
        builder.Property(item => item.EndAtUtc).Utc();
        builder.Property(item => item.PaymentExpiresAtUtc).Utc();
        builder.Property(item => item.PerformedAtUtc).NullableUtc();
        builder.Property(item => item.CompletedAtUtc).NullableUtc();
        builder.Property(item => item.CancelledAtUtc).NullableUtc();
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne<ConsultationOffering>().WithMany().HasForeignKey(item => item.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ConsultationAvailabilitySlot>().WithMany().HasForeignKey(item => item.SlotId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LawyerProfile>().WithMany().HasForeignKey(item => item.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.SlotId).IsUnique()
            .HasFilter("[Status] IN (0,1,2,3,6)");
        builder.HasIndex(item => new { item.ClientId, item.Status, item.StartAtUtc });
        builder.HasIndex(item => new { item.LawyerId, item.Status, item.StartAtUtc });
        builder.HasCheckConstraint("CK_ConsultationBookings_Amounts", "[GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [LawyerNetAmount]");
        builder.HasCheckConstraint("CK_ConsultationBookings_Currency", "[Currency] = 'EGP'");
    }
}

public sealed class ConsultationPaymentTransactionConfiguration
    : IEntityTypeConfiguration<ConsultationPaymentTransaction>
{
    public void Configure(EntityTypeBuilder<ConsultationPaymentTransaction> builder)
    {
        builder.ToTable("ConsultationPaymentTransactions");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.OperationType).HasConversion<int>();
        builder.Property(item => item.Status).HasConversion<int>();
        builder.Property(item => item.ProviderName).IsRequired().IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.IdempotencyKey).IsRequired().IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.ProviderTransactionId).IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.RelatedProviderTransactionId).IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.ProviderStatus).IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.FailureReason).NullableUnicode(1_000);
        builder.Property(item => item.Amount).Money();
        builder.Property(item => item.Currency).IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.ProcessedAtUtc).NullableUtc();
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne<ConsultationBooking>().WithMany().HasForeignKey(item => item.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.ProviderName, item.IdempotencyKey }).IsUnique();
        builder.HasIndex(item => item.ProviderTransactionId);
        builder.HasIndex(item => new { item.BookingId, item.OperationType, item.Status });
        builder.HasCheckConstraint("CK_ConsultationPaymentTransactions_Amount", "[Amount] > 0");
        builder.HasCheckConstraint("CK_ConsultationPaymentTransactions_Currency", "[Currency] = 'EGP'");
    }
}

public sealed class ConsultationEscrowHoldConfiguration
    : IEntityTypeConfiguration<ConsultationEscrowHold>
{
    public void Configure(EntityTypeBuilder<ConsultationEscrowHold> builder)
    {
        builder.ToTable("ConsultationEscrowHolds");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasConversion<int>();
        builder.Property(item => item.GrossAmount).Money();
        builder.Property(item => item.PlatformFeeAmount).Money();
        builder.Property(item => item.NetAmount).Money();
        builder.Property(item => item.Currency).IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.FundedAtUtc).Utc();
        builder.Property(item => item.HoldStartsAtUtc).NullableUtc();
        builder.Property(item => item.HoldExpiresAtUtc).NullableUtc();
        builder.Property(item => item.FrozenAtUtc).NullableUtc();
        builder.Property(item => item.SettledAtUtc).NullableUtc();
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne<ConsultationBooking>().WithMany().HasForeignKey(item => item.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ConsultationPaymentTransaction>().WithMany()
            .HasForeignKey(item => item.DepositTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.BookingId).IsUnique();
        builder.HasCheckConstraint("CK_ConsultationEscrowHolds_Amounts", "[GrossAmount] > 0 AND [GrossAmount] = [PlatformFeeAmount] + [NetAmount]");
    }
}

public sealed class ConsultationLedgerEntryConfiguration
    : IEntityTypeConfiguration<ConsultationLedgerEntry>
{
    public void Configure(EntityTypeBuilder<ConsultationLedgerEntry> builder)
    {
        builder.ToTable("ConsultationLedgerEntries");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.TransactionType).HasConversion<int>();
        builder.Property(item => item.Amount).Money();
        builder.Property(item => item.RunningBalance).Money();
        builder.Property(item => item.Currency).IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.Description).IsRequired().Unicode(500);
        builder.Property(item => item.CreatedAt).Utc();
        builder.HasOne<ConsultationBooking>().WithMany().HasForeignKey(item => item.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ConsultationPaymentTransaction>().WithMany()
            .HasForeignKey(item => item.PaymentTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.BookingId, item.CreatedAt });
        builder.HasCheckConstraint("CK_ConsultationLedgerEntries_Amount", "[Amount] > 0");
    }
}
