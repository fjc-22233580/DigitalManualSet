using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.Services;
using System.Text.Json;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Generates the customer-facing splash screen and copies the required
/// CSS and JavaScript assets to the package output.
/// </summary>
public sealed class SplashScreenGenerator : ISplashScreenGenerator
{
    // Embedded resources stored within the Infrastructure assembly.
    private const string StylesResource = "DigitalManualSet.Infrastructure.Assets.Splash.styles.css";
    private const string ScriptResource = "DigitalManualSet.Infrastructure.Assets.Splash.app.js";
    private const string LogoResource = "DigitalManualSet.Infrastructure.Assets.Splash.company-logo.svg";

    /// <summary>
    /// Creates the splash screen structure within the package output.
    /// </summary>
    public async Task GenerateAsync(Package package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (package.Output is null)
        {
            throw new InvalidOperationException("Package output has not been configured.");
        }

        var rootPath = package.Output.RootPath;

        var cssDirectory = Path.Combine(rootPath, "assets", "css");
        var jsDirectory = Path.Combine(rootPath, "assets", "js");
        var imageDirectory = Path.Combine(rootPath, "assets", "images");

        Directory.CreateDirectory(cssDirectory);
        Directory.CreateDirectory(jsDirectory);
        Directory.CreateDirectory(imageDirectory);

        await CopyEmbeddedResourceAsync(StylesResource, Path.Combine(cssDirectory, "styles.css"), cancellationToken);
        await CopyEmbeddedResourceAsync(ScriptResource, Path.Combine(jsDirectory, "app.js"), cancellationToken);
        await CopyEmbeddedResourceAsync(LogoResource, Path.Combine(imageDirectory, "company-logo.svg"), cancellationToken);
        await GenerateIndexAsync(package, cancellationToken);
    }


    /// <summary>
    /// Generates index.html and embeds the package document data into the page.
    /// </summary>

    private static async Task GenerateIndexAsync(
        Package package,
        CancellationToken cancellationToken)
    {
        // Only include documents with an output path.
        var documents = package.Documents
            .Where(d => !string.IsNullOrWhiteSpace(d.OutputRelativePath))
            .Select(document => new
            {
                number = document.DocumentNumber,
                name = document.DocumentName,
                revision = document.Revision,
                type = document.Type.ToString(),
                fileName = Path.GetFileName(document.OutputRelativePath!),
                path = document.OutputRelativePath!.Replace('\\', '/')
            })
            .ToArray();

        var packageData = new
        {
            order = new
            {
                number = package.Order?.OrderNumber ?? "",
                customer = package.Order?.CustomerName ?? "",
                systemId = package.Order?.SystemId ?? ""
            },

            generatedOn = DateTimeOffset.Now.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
            documents
        };

        var json = JsonSerializer.Serialize(packageData);

        json = json.Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);

        var html = $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">

            <title>Digital Documentation</title>

            <link rel="stylesheet" href="assets/css/styles.css">
        </head>

        <body>
            <main class="container">

                <header class="page-header">
                    <img
                        class="company-logo"
                        src="assets/images/company-logo.svg"
                        alt="Instron logo">

                    <h1>Digital Documentation</h1>
                    <p>Documentation supplied with your equipment.</p>
                </header>

                <section class="order-summary">
                    <h2>Order Information</h2>

                    <div class="order-details">
                        <div>
                            <span>Order Number</span>
                            <strong id="order-number"></strong>
                        </div>

                        <div>
                            <span>Customer</span>
                            <strong id="customer-name"></strong>
                        </div>

                        <div>
                            <span>System ID</span>
                            <strong id="system-id"></strong>
                        </div>

                        <div>
                            <span>Generated</span>
                            <strong id="generated-on"></strong>
                        </div>
                    </div>
                </section>

                <section class="documents-section">
                    <div class="section-heading">
                        <h2>Documents</h2>
                        <span id="document-count"></span>
                    </div>

                    <input
                        id="search"
                        type="search"
                        aria-label="Search documentation"
                        placeholder="Search by number, name or type...">

                    <div id="document-list"></div>
                </section>

                <footer>
                    <p>
                        This documentation package is available offline.
                        Select Open to view a document.
                    </p>
                </footer>

            </main>

            <script>
                window.packageData = {{json}};
            </script>

            <script src="assets/js/app.js"></script>
        </body>
        </html>
        """;

        var outputPath = Path.Combine(package.Output!.RootPath, "index.html");
        await File.WriteAllTextAsync(outputPath, html, cancellationToken);
    }


    /// <summary>
    /// Copies an embedded assembly resource to the requested output path.
    /// </summary>
    /// <param name="resourceName">Name of the resource.</param>
    /// <param name="destinationPath">The destination path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private static async Task CopyEmbeddedResourceAsync(string resourceName, string destinationPath, CancellationToken cancellationToken)
    {
        var assembly = typeof(SplashScreenGenerator).Assembly;

        // Retrieve the embedded CSS/JS file from the compiled assembly.
        await using var source = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be found.");

        // Create or replace the corresponding file on the USB.
        await using var destination = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await source.CopyToAsync(destination, cancellationToken);
    }
}