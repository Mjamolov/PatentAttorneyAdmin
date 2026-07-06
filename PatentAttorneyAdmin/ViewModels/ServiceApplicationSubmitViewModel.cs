using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.ViewModels;

public class ServiceApplicationSubmitViewModel
{
    public Dictionary<int, IFormFile> Documents { get; set; } = new();
}

public class ReviewServiceApplicationViewModel
{
    public int Id { get; set; }
    public ServiceApplicationStatus Status { get; set; }
    public string? StaffComment { get; set; }
}
