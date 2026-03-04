namespace frontend.ViewModels;

public class AdminCategoryCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ImageUrl { get; set; }
}

