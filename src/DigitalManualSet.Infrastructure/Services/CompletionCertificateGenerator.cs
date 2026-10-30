using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Cryptography;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Generates a PDF completion certificate for the finished
/// digital documentation package.
/// </summary>
public sealed class CompletionCertificateGenerator : ICompletionCertificateGenerator
{
    private const string ManifestFileName = "Manifest.json";
    private const string CertificateFileName = "Certificate.pdf";

    /// <summary>
    /// Generates the completion certificate asynchronous.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="System.InvalidOperationException">Package output has not been configured.</exception>
    /// <exception cref="System.IO.FileNotFoundException">The manifest must be generated before the completion certificate.</exception>
    public async Task GenerateAsync(Package package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (package.Output is null)
        {
            throw new InvalidOperationException("Package output has not been configured.");
        }

        var manifestPath = Path.Combine(package.Output.RootPath, ManifestFileName);

        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("The manifest must be generated before the completion certificate.", manifestPath);
        }

        // Hash the completed manifest so the certificate can be used
        // to verify that the package metadata has not changed.
        var manifestHash = await CalculateSha256Async(manifestPath, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var certificatePath = Path.Combine(package.Output.RootPath, CertificateFileName);
        var document = CreateCertificate(package, manifestHash);
        document.GeneratePdf(certificatePath);
    }

    /// <summary>
    /// Creates the PDF document layout.
    /// </summary>
    private static IDocument CreateCertificate(Package package, string manifestHash)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(style => style.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header()
                    .PaddingBottom(20)
                    .Column(column =>
                    {
                        column.Item()
                            .Text("Digital Documentation")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Colors.Grey.Darken2);

                        column.Item()
                            .Text("Completion Certificate")
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.Black);
                    });

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(18);

                        column.Item().Text("This certificate confirms that the digital documentation package was generated successfully.");

                        column.Item()
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        column.Item()
                            .Column(details =>
                            {
                                details.Spacing(8);

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("Order Number: ").SemiBold();
                                        text.Span(package.Order!.OrderNumber);
                                    });

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("Customer Name: ").SemiBold();
                                        text.Span(package.Order!.CustomerName);
                                    });

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("System ID: ").SemiBold();
                                        text.Span(package.Order!.SystemId);
                                    });

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("Generated: ").SemiBold();
                                        text.Span($"{DateTime.Now:dd MMMM yyyy HH:mm}");
                                    });

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("Documents supplied: ").SemiBold();
                                        text.Span(package.Documents!.Count.ToString());
                                    });

                                details.Item()
                                    .Text(text =>
                                    {
                                        text.Span("Manifest: ").SemiBold();
                                        text.Span(ManifestFileName);
                                    });
                            });

                        column.Item()
                            .PaddingTop(10)
                            .Text("Package Verification")
                            .FontSize(16)
                            .SemiBold();

                        column.Item().Text("The SHA-256 value below identifies the manifest generated for this package.");

                        column.Item()
                            .Padding(12)
                            .Background(Colors.Grey.Lighten4)
                            .Text(manifestHash)
                            .FontSize(9);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Digital Manual Set")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });
    }

    /// <summary>
    /// Calculates the SHA-256 hash of the supplied file.
    /// </summary>
    private static async Task<string> CalculateSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}