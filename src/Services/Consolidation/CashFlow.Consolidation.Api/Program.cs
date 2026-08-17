using CashFlow.Consolidation.Api.Middlewares;
using CashFlow.Consolidation.Application;
using CashFlow.Consolidation.Infrastructure;
using CashFlow.Consolidation.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CashFlow - Consolidation API (Saldo Diário Consolidado)",
        Version = "v1",
        Description = "API responsável pela consulta de saldos consolidados e geração de relatórios de fluxo de caixa com alta performance e suporte a picos de tráfego."
    });
});

builder.Services.AddHealthChecks();

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure DB Created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consolidation API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

namespace CashFlow.Consolidation.Api
{
    public partial class Program { }
}
