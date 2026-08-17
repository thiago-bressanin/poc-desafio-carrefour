using CashFlow.Transactions.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Transactions.Infrastructure.Persistence;

public class TransactionsDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);

        // MassTransit Transactional Outbox schema configuration
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
