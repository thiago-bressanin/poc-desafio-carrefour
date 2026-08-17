using CashFlow.Consolidation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Configurations;

public class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("DailyBalances");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.MerchantId)
            .IsRequired();

        builder.Property(b => b.Date)
            .IsRequired();

        builder.Property(b => b.TotalCredits)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.TotalDebits)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.TotalTransactions)
            .IsRequired();

        builder.Property(b => b.LastUpdatedAt)
            .IsRequired();

        builder.HasIndex(b => new { b.MerchantId, b.Date })
            .IsUnique();
    }
}
