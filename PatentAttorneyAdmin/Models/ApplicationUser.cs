using Microsoft.AspNetCore.Identity;

namespace PatentAttorneyAdmin.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string PassportSeries { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string? PassportIssuedBy { get; set; }
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string Inn { get; set; } = string.Empty;
    public string PassportAddress { get; set; } = string.Empty;
    public string ActualAddress { get; set; } = string.Empty;
    public string? PassportFrontPath { get; set; }
    public string? PassportBackPath { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClientRequest> ClientRequests { get; set; } = new List<ClientRequest>();
    public ICollection<ServiceApplication> ServiceApplications { get; set; } = new List<ServiceApplication>();
}
