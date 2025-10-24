namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class CategoryDto
    {
        public int CourseCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
