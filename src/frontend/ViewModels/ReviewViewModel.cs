using System.ComponentModel.DataAnnotations;

namespace frontend.ViewModels;

public class ReviewViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCreateViewModel
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public string Slug { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Комментарий слишком длинный")]
    public string? Comment { get; set; }
}

