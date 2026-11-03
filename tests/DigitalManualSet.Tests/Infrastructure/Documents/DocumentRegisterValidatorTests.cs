
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class DocumentRegisterValidatorTests
{
    [Fact]
    public void Validate_NullDocuments_ThrowsArgumentNullException()
    {
        var validator = CreateValidator();

        Assert.Throws<ArgumentNullException>(() =>
            validator.Validate(null!));
    }

    [Fact]
    public void Validate_EmptyDocuments_ReturnsEmpty()
    {
        var validator = CreateValidator();

        var result = validator.Validate([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_MatchingDocument_AppliesRegisterMetadata()
    {
        var validator = CreateValidator(
            ("F123456", "Official Drawing", "Production", "B", "Drawing"));

        var document = CreateDocument("F123456");

        var result = validator.Validate([document]);

        var validated = Assert.Single(result);

        Assert.Equal("Official Drawing", validated.DocumentName);
        Assert.Equal("B", validated.Revision);
        Assert.Equal("Production", validated.LifecyclePhase);
        Assert.Equal(DocumentRegisterStatus.Active, validated.RegisterStatus);
        Assert.Equal(DocumentType.Drawing, validated.Type);
    }

    [Fact]
    public void Validate_DocumentNotInRegister_SetsNotFound()
    {
        var validator = CreateValidator();

        var document = CreateDocument("F123456");
        document.LifecyclePhase = "Production";

        validator.Validate([document]);

        Assert.Equal(DocumentRegisterStatus.NotFound, document.RegisterStatus);
        Assert.Null(document.LifecyclePhase);
    }

    [Fact]
    public void Validate_RegisterLookup_IsCaseInsensitive()
    {
        var validator = CreateValidator(
            ("F123456", "Official Drawing", "Production", "A", "Drawing"));

        var document = CreateDocument("f123456");

        validator.Validate([document]);

        Assert.Equal(DocumentRegisterStatus.Active, document.RegisterStatus);
        Assert.Equal("Official Drawing", document.DocumentName);
    }

    [Fact]
    public void Validate_DocumentNumberWithWhitespace_TrimsForLookup()
    {
        var validator = CreateValidator(
            ("F123456", "Official Drawing", "Production", "A", "Drawing"));

        var document = CreateDocument("  F123456  ");

        validator.Validate([document]);

        Assert.Equal(DocumentRegisterStatus.Active, document.RegisterStatus);
    }

    [Fact]
    public void Validate_EmptyRegisterDescription_PreservesOriginalName()
    {
        var validator = CreateValidator(
            ("F123456", "", "Production", "A", "Drawing"));

        var document = CreateDocument("F123456");

        validator.Validate([document]);

        Assert.Equal("Original Name", document.DocumentName);
    }

    [Theory]
    [InlineData("Production", DocumentRegisterStatus.Active)]
    [InlineData("Custom", DocumentRegisterStatus.Active)]
    [InlineData("Obsolete", DocumentRegisterStatus.Obsolete)]
    [InlineData("Development", DocumentRegisterStatus.RequiresReview)]
    [InlineData("Unknown", DocumentRegisterStatus.RequiresReview)]
    public void Validate_LifecyclePhase_SetsCorrectStatus(
        string lifecycle,
        DocumentRegisterStatus expected)
    {
        var validator = CreateValidator(
            ("F123456", "Drawing", lifecycle, "A", "Drawing"));

        var document = CreateDocument("F123456");

        validator.Validate([document]);

        Assert.Equal(expected, document.RegisterStatus);
    }

    [Fact]
    public void Validate_LifecyclePhase_IsCaseInsensitive()
    {
        var validator = CreateValidator(
            ("F123456", "Drawing", "production", "A", "Drawing"));

        var document = CreateDocument("F123456");

        validator.Validate([document]);

        Assert.Equal(DocumentRegisterStatus.Active, document.RegisterStatus);
    }

    [Fact]
    public void Validate_MissingRevision_RequiresReview()
    {
        var validator = CreateValidator(
            ("F123456", "Drawing", "Production", "", "Drawing"));

        var document = CreateDocument("F123456");

        validator.Validate([document]);

        Assert.Equal(
            DocumentRegisterStatus.RequiresReview,
            document.RegisterStatus);
    }

    [Theory]
    [InlineData("Drawing", DocumentType.Drawing)]
    [InlineData("DRAWINGS", DocumentType.Drawing)]
    [InlineData("(Drawing)", DocumentType.Drawing)]
    [InlineData("Manual", DocumentType.Manual)]
    [InlineData("MANUALS", DocumentType.Manual)]
    [InlineData("Certificate", DocumentType.Certificate)]
    [InlineData("CERT", DocumentType.Certificate)]
    [InlineData("(CERTIFICATES)", DocumentType.Certificate)]
    public void Validate_PartType_DeterminesDocumentType(
        string partType,
        DocumentType expected)
    {
        var validator = CreateValidator(
            ("X123456", "Document", "Production", "A", partType));

        var document = CreateDocument("X123456");

        validator.Validate([document]);

        Assert.Equal(expected, document.Type);
    }

    [Theory]
    [InlineData("F123456", DocumentType.Drawing)]
    [InlineData("M123456", DocumentType.Manual)]
    [InlineData("HS123456", DocumentType.Manual)]
    [InlineData("X123456", DocumentType.Other)]
    public void Validate_UnknownPartType_FallsBackToDocumentPrefix(
        string number,
        DocumentType expected)
    {
        var validator = CreateValidator(
            (number, "Document", "Production", "A", "Unknown"));

        var document = CreateDocument(number);

        validator.Validate([document]);

        Assert.Equal(expected, document.Type);
    }

    [Fact]
    public void Validate_RegisterPartType_OverridesDocumentPrefix()
    {
        var validator = CreateValidator(
            ("F123456", "Certificate", "Production", "A", "Certificate"));

        var document = CreateDocument("F123456");

        validator.Validate([document]);

        Assert.Equal(DocumentType.Certificate, document.Type);
    }

    [Fact]
    public void Validate_MultipleDocuments_ValidatesIndividually()
    {
        var validator = CreateValidator(
            ("F123456", "Drawing", "Production", "A", "Drawing"),
            ("M654321", "Manual", "Obsolete", "B", "Manual"));

        var documents = new[]
        {
            CreateDocument("F123456"),
            CreateDocument("M654321"),
            CreateDocument("HS999999")
        };

        var result = validator.Validate(documents);

        Assert.Equal(3, result.Count);

        Assert.Equal(
            DocumentRegisterStatus.Active,
            documents[0].RegisterStatus);

        Assert.Equal(
            DocumentRegisterStatus.Obsolete,
            documents[1].RegisterStatus);

        Assert.Equal(
            DocumentRegisterStatus.NotFound,
            documents[2].RegisterStatus);
    }

    [Fact]
    public void Validate_ReturnsSameDocumentCollection()
    {
        var validator = CreateValidator();

        IReadOnlyList<Document> documents =
        [
            CreateDocument("F123456")
        ];

        var result = validator.Validate(documents);

        Assert.Same(documents, result);
    }

    // Test helpers

    private static Document CreateDocument(string number)
    {
        return new Document(
            number,
            "Original Name",
            "A",
            "Drawing");
    }

    private static DocumentRegisterValidator CreateValidator(
        params (string Number, string Description,
                string Lifecycle, string Revision,
                string PartType)[] entries)
    {
        var register = entries.ToDictionary(
            entry => entry.Number,
            entry => new DocumentRegisterEntry(
                entry.Number,
                entry.Description,
                entry.Lifecycle,
                entry.Revision,
                entry.PartType),
            StringComparer.OrdinalIgnoreCase);

        return new DocumentRegisterValidator(
            new FakeDocumentRegisterProvider(register));
    }

    /// <summary>
    /// Fake register provider for unit testing.
    /// </summary>
    private sealed class FakeDocumentRegisterProvider : IDocumentRegisterProvider
    {
        public IReadOnlyDictionary<string, DocumentRegisterEntry> Register { get; }

        public FakeDocumentRegisterProvider(
            IReadOnlyDictionary<string, DocumentRegisterEntry> register)
        {
            Register = register;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
