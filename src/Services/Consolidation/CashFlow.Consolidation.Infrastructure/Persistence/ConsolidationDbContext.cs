using CashFlow.Consolidation.Domain;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Persistence;

public class ConsolidationDbContext : DbContext
{
    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();
    public DbSet<ProcessedTransaction> ProcessedTransactions => Set<ProcessedTransaction>();

    public ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsolidationDbContext).Assembly);
    }
}
