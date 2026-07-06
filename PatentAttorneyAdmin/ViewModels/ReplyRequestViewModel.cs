using System.ComponentModel.DataAnnotations;

namespace PatentAttorneyAdmin.ViewModels;

public class ReplyRequestViewModel
{
    public int RequestId { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Message")]
    public string Body { get; set; } = string.Empty;

    [Display(Name = "Attachment")]
    public IFormFile? Attachment { get; set; }
}
