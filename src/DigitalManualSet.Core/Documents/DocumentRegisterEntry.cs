namespace DigitalManualSet.Core.Documents;

public record DocumentRegisterEntry(
    string Number,
    string Description,
    string LifecyclePhase,
    string Revision,
    string PartType)
{
    public bool IsObsolete =>
        string.Equals(
            LifecyclePhase,
            "Obsolete",
            StringComparison.OrdinalIgnoreCase);
}