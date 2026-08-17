using CashFlow.Transactions.Domain;
using CashFlow.Transactions.Infrastructure.Persistence;
using CashFlow.Transactions.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Transactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=transactions.db";

        services.AddDbContext<TransactionsDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                // PostgreSQL if configured
                options.UseNpgsql(connectionString);
            }
            else
            {
                // Default to SQLite for zero-setup local dev/test portability
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Configure MassTransit with Transactional Outbox Pattern
        var rabbitMqHost = configuration["RabbitMQ:Host"];

        services.AddMassTransit(busConfig =>
        {
            busConfig.AddEntityFrameworkOutbox<TransactionsDbContext>(o =>
            {
                o.UseSqlite();
                o.UseBusOutbox();
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
            });

            if (!string.IsNullOrWhiteSpace(rabbitMqHost))
            {
                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                busConfig.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
