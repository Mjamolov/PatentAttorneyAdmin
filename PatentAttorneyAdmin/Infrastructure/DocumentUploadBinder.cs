namespace PatentAttorneyAdmin.Infrastructure;

public static class DocumentUploadBinder
{
    private const string Prefix = "Documents[";

    public static Dictionary<int, IFormFile> BindDocuments(IFormFileCollection files)
    {
        var result = new Dictionary<int, IFormFile>();

        foreach (var file in files)
        {
            if (file.Length == 0 || !TryParseDocumentKey(file.Name, out var key))
                continue;

            result[key] = file;
        }

        return result;
    }

    private static bool TryParseDocumentKey(string fieldName, out int key)
    {
        key = 0;
        if (!fieldName.StartsWith(Prefix, StringComparison.Ordinal) || !fieldName.EndsWith(']'))
            return false;

        return int.TryParse(fieldName[Prefix.Length..^1], out key);
    }
}
