namespace PatentAttorneyAdmin.Infrastructure;

public static class UploadFileTypes
{
    public static readonly string[] Extensions =
    {
        ".jpg",
        ".jpeg",
        ".pdf",
        ".doc",
        ".docx"
    };

    public const string AcceptAttribute =
        ".jpg,.jpeg,.pdf,.doc,.docx,image/jpeg,application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public static bool IsAllowed(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        return Extensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
    }
}
