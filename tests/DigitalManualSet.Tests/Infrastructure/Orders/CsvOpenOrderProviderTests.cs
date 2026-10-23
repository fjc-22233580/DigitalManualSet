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
    /// Verifies that only unique order numbers with valid System IDs are returned.
    /// </summary>
    [Fact]
    public async Task GetOpenOrdersAsync_WithValidCsv_ReturnsUniqueOrdersWithValidSystemIds()
    {
        // Arrange
        var provider = new CsvOpenOrderProvider(GetTestDataPath("ValidOpenOrders.csv"));

        // Act
        var orders = await provider.GetOpenOrdersAsync();

        // Assert
        Assert.Equal(
            [
                "A123456", "A123457", "A123458", "A123460", "A123461", "A123463", "A123464", "A123465", "A123466", "A123467",
                "A123469", "A123470", "A123472", "A123474", "A123475", "A123476", "A123478", "A123479", "A123480", "A123482"
            ],
            orders.Select(order => order.OrderNumber));
    }

    /// <summary>
    /// Verifies that rows with an invalid or empty System ID are skipped.
    /// </summary>
    [Theory]
    [InlineData("STOCK")]
    [InlineData("INVALID123")]
    [InlineData("ABC123")]
    [InlineData("")]
    public async Task GetOpenOrdersAsync_WhenSystemIdIsInvalid_SkipsRow(string systemId)
    {
        // Arrange
        var csvFile = Path.Combine(Path.GetTempPath(), $"invalid_system_id_{Guid.NewGuid()}.csv");
        await File.WriteAllTextAsync(csvFile, $"Header\nA1,Customer,15/01/2030,X-1,1,{systemId}");

        try
        {
            var provider = new CsvOpenOrderProvider(csvFile);

            // Act
            var orders = await provider.GetOpenOrdersAsync();

            // Assert
            Assert.Empty(orders);
        }
        finally
        {
            File.Delete(csvFile);
        }
    }

    /// <summary>
    /// Verifies that the first valid row for a duplicated order number is kept.
    /// </summary>
    [Fact]
    public async Task GetOpenOrdersAsync_WhenOrderNumberIsDuplicated_ReturnsFirstValidRow()
    {
        // Arrange
        var csvFile = Path.Combine(Path.GetTempPath(), $"duplicates_{Guid.NewGuid()}.csv");
        await File.WriteAllTextAsync(
            csvFile,
            "Header\nA1,Customer,15/01/2030,X-1,1,STOCK\nA1,Customer,15/01/2030,X-2,1,E3BMTB1\nA1,Customer,15/01/2030,X-3,1,8801MTB2");

        try
        {
            var provider = new CsvOpenOrderProvider(csvFile);

            // Act
            var orders = await provider.GetOpenOrdersAsync();

            // Assert
            var order = Assert.Single(orders);
            Assert.Equal("E3BMTB1", order.SystemId);
        }
        finally
        {
            File.Delete(csvFile);
        }
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
        await File.WriteAllTextAsync(csvFile, "Header\n123,,15/01/2030,A620-822,1,8801MTB18452");

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