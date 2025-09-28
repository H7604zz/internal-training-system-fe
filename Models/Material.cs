namespace InternalTrainingSystem.WebApp.Models
{
    public class Material
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaterialType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int CourseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum MaterialType
    {
        Document = 1,
        Video = 2,
        Audio = 3,
        Presentation = 4,
        Image = 5,
        Other = 6
    }
}