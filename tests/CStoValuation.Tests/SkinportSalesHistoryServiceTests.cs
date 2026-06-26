using CStoValuation.Infrastructure.Skinport;
using CStoValuation.Tests.TestSupport;
using Microsoft.Extensions.Time.Testing;

namespace CStoValuation.Tests;

public class SkinportSalesHistoryServiceTests
{
    private const string Skinport = "https://api.skinport.com/";

    [Fact]
    public async Task Parses_trailing_windows_keyed_by_name()
    {
        var service = new SkinportSalesHistoryService(
            MockHttp.ClientReturning(Skinport, Fixtures.Read("skinport-sales-history.json")));

        var history = await service.GetSalesHistoryAsync("EUR");

        Assert.Equal(2, history.Count);

        var ak = history["AK-47 | Redline (Field-Tested)"];
        Assert.Equal("EUR", ak.Currency);
        Assert.Equal(12.5m, ak.Last7Days.Average);
        Assert.Equal(11.0m, ak.Last30Days.Average);
        Assert.Equal(10.5m, ak.Last90Days.Average);
        Assert.Equal(5, ak.Last24Hours.Volume);

        var empty = history["No Sales Item"];
        Assert.Null(empty.Last7Days.Average);
        Assert.Equal(0, empty.Last7Days.Volume);
    }

    [Fact]
    public async Task Caches_results_for_five_minutes()
    {
        var callCount = 0;
        var client = MockHttp.Client(Skinport, _ =>
        {
            callCount++;
            return MockHttp.Response(Fixtures.Read("skinport-sales-history.json"));
        });
        var clock = new FakeTimeProvider(DateTimeOffset.UnixEpoch);
        var service = new SkinportSalesHistoryService(client, clock);

        await service.GetSalesHistoryAsync("EUR");
        await service.GetSalesHistoryAsync("EUR");
        Assert.Equal(1, callCount);

        clock.Advance(TimeSpan.FromMinutes(6));
        await service.GetSalesHistoryAsync("EUR");
        Assert.Equal(2, callCount);
    }
}
