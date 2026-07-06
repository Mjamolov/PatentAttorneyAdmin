namespace PatentAttorneyAdmin.Models;

public class ClientRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int? ServiceCategoryId { get; set; }
    public ServiceCategory? ServiceCategory { get; set; }
    public string Subject { get; set; } = string.Empty;
    public RequestType Type { get; set; } = RequestType.General;
    public RequestStatus Status { get; set; } = RequestStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RequestMessage> Messages { get; set; } = new List<RequestMessage>();
}
