namespace PatentAttorneyAdmin.Models;

public class ServiceApplication
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int ServiceCategoryId { get; set; }
    public ServiceCategory ServiceCategory { get; set; } = null!;
    public ServiceApplicationStatus Status { get; set; } = ServiceApplicationStatus.Submitted;
    public string? StaffComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public ICollection<ServiceApplicationDocument> Documents { get; set; } = new List<ServiceApplicationDocument>();
}
