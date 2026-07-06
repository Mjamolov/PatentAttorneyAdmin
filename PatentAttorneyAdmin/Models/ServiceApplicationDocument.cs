namespace PatentAttorneyAdmin.Models;

public class ServiceApplicationDocument
{
    public int Id { get; set; }
    public int ServiceApplicationId { get; set; }
    public ServiceApplication ServiceApplication { get; set; } = null!;
    public ServiceDocumentType DocumentType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool NeedsCorrection { get; set; }
    public string? StaffComment { get; set; }
}
