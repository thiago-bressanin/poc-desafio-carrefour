using System.Net;
using System.Net.Http.Json;
using CashFlow.IntegrationTests.Fixtures;
using CashFlow.Transactions.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace CashFlow.IntegrationTests.Controllers;

public class TransactionsApiTests : IClassFixture<TransactionsApiFactory>
{
    private readonly HttpClient _client;

    public TransactionsApiTests(TransactionsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateTransactionRequestDto
        {
            MerchantId = Guid.NewGuid(),
            Type = "Credit",
            Amount = 1500.50m,
            Date = DateTime.UtcNow.Date,
            Description = "Aporte inicial de caixa"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.MerchantId.Should().Be(request.MerchantId);
        created.Amount.Should().Be(request.Amount);
        created.Type.Should().Be("Credit");

        // Act - Query By Id
        var getResponse = await _client.GetAsync($"/api/v1/transactions/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateTransactionRequestDto
        {
            MerchantId = Guid.NewGuid(),
            Type = "Credit",
            Amount = -50m,
            Date = DateTime.UtcNow.Date,
            Description = "Valor negativo inválido"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
