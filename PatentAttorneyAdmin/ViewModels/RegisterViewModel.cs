using System.ComponentModel.DataAnnotations;

namespace PatentAttorneyAdmin.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Required")]
    [Display(Name = "FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "LastName")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "MiddleName")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "PassportSeries")]
    public string PassportSeries { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "PassportNumber")]
    public string PassportNumber { get; set; } = string.Empty;

    [Display(Name = "PassportIssuedBy")]
    public string? PassportIssuedBy { get; set; }

    [Display(Name = "PassportIssuedDate")]
    [DataType(DataType.Date)]
    public DateTime? PassportIssuedDate { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "PassportExpiryDate")]
    [DataType(DataType.Date)]
    public DateTime? PassportExpiryDate { get; set; }

    [Required(ErrorMessage = "Required")]
    [StringLength(20, MinimumLength = 9, ErrorMessage = "InvalidInn")]
    [Display(Name = "Inn")]
    public string Inn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "PassportAddress")]
    public string PassportAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "ActualAddress")]
    public string ActualAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^\+?[\d\s\-()]{9,20}$", ErrorMessage = "InvalidPhone")]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [EmailAddress(ErrorMessage = "InvalidEmail")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "PassportFront")]
    public IFormFile? PassportFront { get; set; }

    [Display(Name = "PassportBack")]
    public IFormFile? PassportBack { get; set; }
}
