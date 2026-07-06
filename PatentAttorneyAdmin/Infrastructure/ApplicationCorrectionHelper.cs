using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Infrastructure;

public static class ApplicationCorrectionHelper
{
    public static bool HasPendingCorrections(ServiceApplication application) =>
        application.Status == ServiceApplicationStatus.NeedsCorrection ||
        application.Documents.Any(d => d.NeedsCorrection);

    public static HashSet<int> GetDocumentsToCorrect(ServiceApplication application)
    {
        var marked = application.Documents
            .Where(d => d.NeedsCorrection)
            .Select(d => (int)d.DocumentType)
            .ToHashSet();

        if (marked.Count > 0)
            return marked;

        return application.Documents
            .Select(d => (int)d.DocumentType)
            .ToHashSet();
    }

    public static ServiceApplicationDocument? GetExistingDocument(
        ServiceApplication application,
        ServiceDocumentType type) =>
        application.Documents.FirstOrDefault(d => d.DocumentType == type);
}
