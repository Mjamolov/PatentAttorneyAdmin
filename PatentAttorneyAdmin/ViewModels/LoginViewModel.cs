using System.ComponentModel.DataAnnotations;

namespace PatentAttorneyAdmin.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Required")]
    [EmailAddress(ErrorMessage = "InvalidEmail")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "RememberMe")]
    public bool RememberMe { get; set; }
}
