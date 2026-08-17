using CashFlow.Transactions.Api.Middlewares;
using CashFlow.Transactions.Application;
using CashFlow.Transactions.Infrastructure;
using CashFlow.Transactions.Infrastructure.Persistence;
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

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CashFlow - Transactions API (Gestão de Lançamentos)",
        Version = "v1",
        Description = "API responsável pelo registro e consulta de lançamentos financeiros (Créditos e Débitos) com alta disponibilidade e padrão Transactional Outbox."
    });
});

builder.Services.AddHealthChecks();

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply DB Migrations / ensure DB created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transactions API v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

namespace CashFlow.Transactions.Api
{
    public partial class Program { }
}
