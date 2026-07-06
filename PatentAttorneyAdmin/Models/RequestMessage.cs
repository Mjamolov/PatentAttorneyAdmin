namespace PatentAttorneyAdmin.Models;

public class RequestMessage
{
    public int Id { get; set; }
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public string? SenderUserId { get; set; }
    public ApplicationUser? SenderUser { get; set; }
    public bool IsFromStaff { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
