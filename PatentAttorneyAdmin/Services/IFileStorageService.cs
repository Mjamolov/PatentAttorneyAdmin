namespace PatentAttorneyAdmin.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder);
    string GetWebPath(string storedPath);
}
