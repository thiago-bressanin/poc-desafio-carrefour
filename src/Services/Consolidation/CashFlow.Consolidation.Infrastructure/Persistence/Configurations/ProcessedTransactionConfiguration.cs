using CashFlow.Consolidation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Configurations;

public class ProcessedTransactionConfiguration : IEntityTypeConfiguration<ProcessedTransaction>
{
    public void Configure(EntityTypeBuilder<ProcessedTransaction> builder)
    {
        builder.ToTable("ProcessedTransactions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProcessedAt)
            .IsRequired();
    }
}
