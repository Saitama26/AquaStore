using System.ComponentModel.DataAnnotations;

namespace frontend.ViewModels;

public class ConfirmRegistrationViewModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Код обязателен")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен состоять из 6 цифр")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Код должен содержать только цифры")]
    public string Code { get; set; } = string.Empty;
}

