namespace InternalTrainingSystem.WebApp.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Duration { get; set; } // in hours
        public CourseLevel Level { get; set; }
        public CourseStatus Status { get; set; }
        public int TrainerId { get; set; }
        public User? Trainer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Material>? Materials { get; set; }
    }

    public enum CourseLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3
    }

    public enum CourseStatus
    {
        Draft = 1,
        Published = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }
}