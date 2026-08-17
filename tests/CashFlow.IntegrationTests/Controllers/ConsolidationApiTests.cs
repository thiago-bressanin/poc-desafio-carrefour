using System.Net;
using System.Net.Http.Json;
using CashFlow.Consolidation.Application.DTOs;
using CashFlow.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace CashFlow.IntegrationTests.Controllers;

public class ConsolidationApiTests : IClassFixture<ConsolidationApiFactory>
{
    private readonly HttpClient _client;

    public ConsolidationApiTests(ConsolidationApiFactory factory)
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
    public async Task GetDailyBalanceByDate_ShouldReturn200WithZeroBalanceForEmptyDay()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var date = "2026-08-16";

        // Act
        var response = await _client.GetAsync($"/api/v1/daily-balances/{date}?merchantId={merchantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var balance = await response.Content.ReadFromJsonAsync<DailyBalanceDto>();
        balance.Should().NotBeNull();
        balance!.MerchantId.Should().Be(merchantId);
        balance.TotalCredits.Should().Be(0m);
        balance.TotalDebits.Should().Be(0m);
        balance.NetBalance.Should().Be(0m);
    }

    [Fact]
    public async Task GetDailyBalanceReport_ShouldReturn200WithReport()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var start = "2026-08-01";
        var end = "2026-08-05";

        // Act
        var response = await _client.GetAsync($"/api/v1/daily-balances?merchantId={merchantId}&startDate={start}&endDate={end}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<DailyBalanceReportDto>();
        report.Should().NotBeNull();
        report!.MerchantId.Should().Be(merchantId);
        report.DailyBalances.Should().HaveCount(5);
    }
}
