using System.ComponentModel.DataAnnotations;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.ViewModels;

public class CreateRequestViewModel
{
    [Required(ErrorMessage = "Required")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Message")]
    public string Body { get; set; } = string.Empty;

    [Display(Name = "RequestType")]
    public RequestType Type { get; set; } = RequestType.General;

    public int? ServiceCategoryId { get; set; }

    [Display(Name = "Attachment")]
    public IFormFile? Attachment { get; set; }
}
