using System.Reflection;

namespace DigitalManualSet.App;

public static class AppInfo
{
    private static readonly Assembly Assembly = typeof(AppInfo).Assembly;

    public static string ProductName =>
        Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
        ?? "Product Name Not Found";
}