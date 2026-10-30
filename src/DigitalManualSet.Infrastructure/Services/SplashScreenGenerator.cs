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

        // Create the folder structure used by the splash screen.
        var cssDirectory = Path.Combine(rootPath, "assets", "css");
        var jsDirectory = Path.Combine(rootPath, "assets", "js");

        Directory.CreateDirectory(cssDirectory);
        Directory.CreateDirectory(jsDirectory);

        // Copy the static CSS and JavaScript resources from the application assembly onto the USB.
        await CopyEmbeddedResourceAsync(StylesResource, Path.Combine(cssDirectory, "styles.css"), cancellationToken);
        await CopyEmbeddedResourceAsync(ScriptResource, Path.Combine(jsDirectory, "app.js"), cancellationToken);

        // Generate the dynamic HTML page using the current package contents.
        await GenerateIndexAsync(package, cancellationToken);
    }

    /// <summary>
    /// Generates index.html and embeds the package document data into the page.
    /// </summary>
    private static async Task GenerateIndexAsync(Package package, CancellationToken cancellationToken)
    {
        // Build the simplified document data required by the splash screen.
        // OutputRelativePath points to the actual file copied to the USB.
        var documents = package.Documents.Select(document =>
            new
            {
                fileName = Path.GetFileName(document.LocalFilePath),
                type = document.Type.ToString(),
                path = document.OutputRelativePath?.Replace('\\', '/')
            });

        var packageData = new
        {
            documents
        };

        // Serialise the document data so JavaScript can use it without
        // needing to fetch Manifest.json from a local file:// URL.
        var json = JsonSerializer.Serialize(packageData,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        // Prevent a filename containing </script> from accidentally terminating the embedded script block.
        json = json.Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);

        // Raw interpolated string allows the generated JSON to be inserted directly into the HTML without extensive escaping.
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

                    <header>
                        <h1>Digital Documentation</h1>
                        <p>Documentation supplied with this machine.</p>
                    </header>

                    <section>
                        <input
                            id="search"
                            type="search"
                            placeholder="Search documentation...">

                        <p id="document-count"></p>

                        <div id="document-list"></div>
                    </section>

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