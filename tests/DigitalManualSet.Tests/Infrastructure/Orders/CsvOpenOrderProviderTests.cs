using DigitalManualSet.Infrastructure.Orders;

namespace DigitalManualSet.Tests.Infrastructure.Orders;

public class CsvOpenOrderProviderTests
{
    private static string GetTestDataPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    /// <summary>
    /// Verifies that the constructor throws when the file path is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenFilepathIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CsvOpenOrderProvider(null!));
    }

    /// <summary>
    /// Verifies that the constructor throws when the file path is empty or whitespace.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenFilepathIsEmptyOrWhitespace_ThrowsArgumentException(string filePath)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CsvOpenOrderProvider(filePath));
    }

    /// <summary>
    /// Verifies that the constructor throws when the file does not exist.
    /// </summary>
    [Fact]
    public void Constructor_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new CsvOpenOrderProvider("nonexistent.csv"));
    }

    /// <summary>
    /// Verifies that valid orders are returned from a well-formed CSV file.
    /// </summary>
    [Fact]
    public async Task GetOpenOrdersAsync_WithValidCsv_ReturnsOrders()
    {
        // Arrange
        var csvFile = GetTestDataPath("ValidOpenOrders.csv");
        var provider = new CsvOpenOrderProvider(csvFile);

        // Act
        var orders = await provider.GetOpenOrdersAsync();

        // Assert
        Assert.NotEmpty(orders);
        Assert.All(orders, order =>
        {
            Assert.NotNull(order.OrderNumber);
            Assert.NotNull(order.CustomerName);
            Assert.NotNull(order.SystemId);
        });
    }

    /// <summary>
    /// Verifies that a row with incorrect column count throws an exception.
    /// </summary>
    [Fact]
    public async Task GetOpenOrdersAsync_WhenRowHasIncorrectColumns_ThrowsInvalidDataException()
    {
        // Arrange
        var csvFile = Path.Combine(Path.GetTempPath(), "invalid_columns.csv");
        await File.WriteAllTextAsync(csvFile, "OrderNumber,CustomerName,SystemID\n123,John Doe");

        try
        {
            var provider = new CsvOpenOrderProvider(csvFile);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetOpenOrdersAsync());
        }
        finally
        {
            File.Delete(csvFile);
        }
    }

    /// <summary>
    /// Verifies that a row with empty values throws an exception.
    /// </summary>
    [Fact]
    public async Task GetOpenOrdersAsync_WhenRowHasEmptyValue_ThrowsInvalidDataException()
    {
        // Arrange
        var csvFile = Path.Combine(Path.GetTempPath(), "empty_value.csv");
        await File.WriteAllTextAsync(csvFile, "OrderNumber,CustomerName,SystemID\n123,,SYS001");

        try
        {
            var provider = new CsvOpenOrderProvider(csvFile);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetOpenOrdersAsync());
        }
        finally
        {
            File.Delete(csvFile);
        }
    }

}