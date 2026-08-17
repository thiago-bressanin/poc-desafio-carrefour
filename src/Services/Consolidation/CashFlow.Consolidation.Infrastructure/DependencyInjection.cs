using CashFlow.Consolidation.Application.Consumers;
using CashFlow.Consolidation.Application.Interfaces;
using CashFlow.Consolidation.Domain;
using CashFlow.Consolidation.Infrastructure.Cache;
using CashFlow.Consolidation.Infrastructure.Persistence;
using CashFlow.Consolidation.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Consolidation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=consolidation.db";

        services.AddDbContext<ConsolidationDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IProcessedTransactionRepository, ProcessedTransactionRepository>();

        // Caching: Redis if available, else MemoryDistributedCache
        var redisConfig = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConfig))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfig;
                options.InstanceName = "CashFlow_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IConsolidationCacheService, ConsolidationCacheService>();

        // MassTransit Consumer Configuration
        var rabbitMqHost = configuration["RabbitMQ:Host"];

        services.AddMassTransit(busConfig =>
        {
            busConfig.AddConsumer<TransactionCreatedConsumer>();

            if (!string.IsNullOrWhiteSpace(rabbitMqHost))
            {
                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("cashflow-consolidation-queue", e =>
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
                        e.ConfigureConsumer<TransactionCreatedConsumer>(context);
                    });
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
