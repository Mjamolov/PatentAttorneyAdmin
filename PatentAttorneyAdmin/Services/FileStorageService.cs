namespace PatentAttorneyAdmin.Services;

using PatentAttorneyAdmin.Infrastructure;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (!UploadFileTypes.IsAllowed(file.FileName))
            throw new InvalidOperationException("Unsupported file type.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(physicalPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine("uploads", subfolder, fileName).Replace('\\', '/');
    }

    public string GetWebPath(string storedPath) => "/" + storedPath.Replace('\\', '/');
}
