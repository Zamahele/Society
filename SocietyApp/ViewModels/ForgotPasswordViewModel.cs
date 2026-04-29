using System.ComponentModel.DataAnnotations;

namespace SocietyApp.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "ID Number is required.")]
    public string IDNumber { get; set; } = string.Empty;
}

public class SecurityQuestionsViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = string.Empty;
}

public class ResetPasswordConfirmViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
