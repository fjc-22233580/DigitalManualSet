
using DigitalManualSet.Infrastructure.Orders;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigitalManualSet.Tests.Infrastructure.Orders;

public class SpreadsheetOpenOrderProviderTests
{
    private readonly SpreadsheetOpenOrderProvider _provider =
        new(NullLogger<SpreadsheetOpenOrderProvider>.Instance);

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SpreadsheetOpenOrderProvider(null!));
    }

    [Fact]
    public void Orders_BeforeInitialization_IsNull()
    {
        Assert.Null(_provider.Orders);
    }

    [Fact]
    public async Task GetOpenOrdersAsync_BeforeInitialization_ReturnsEmpty()
    {
        var result = await _provider.GetOpenOrdersAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task InitializeAsync_NonExistentFile_LeavesOrdersNull()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.xlsx");

        await _provider.InitializeAsync(path);

        Assert.Null(_provider.Orders);
    }

    [Fact]
    public async Task InitializeAsync_InvalidFile_LeavesOrdersNull()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.xlsx");

        try
        {
            await File.WriteAllTextAsync(path, "Invalid Excel content");

            await _provider.InitializeAsync(path);

            Assert.Null(_provider.Orders);
            Assert.Empty(await _provider.GetOpenOrdersAsync());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task GetOpenOrdersAsync_CancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _provider.GetOpenOrdersAsync(cts.Token));
    }

    [Fact]
    public async Task InitializeAsync_CancelledToken_Throws()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.xlsx");

        try
        {
            await File.WriteAllTextAsync(path, "Test content");

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => _provider.InitializeAsync(path, cts.Token));

            Assert.Null(_provider.Orders);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
