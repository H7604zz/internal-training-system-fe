namespace InternalTrainingSystem.WebApp.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public User? User { get; set; }
        public Course? Course { get; set; }
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public bool IsCertified { get; set; }
    }

    public enum EnrollmentStatus
    {
        Enrolled = 1,
        InProgress = 2,
        Completed = 3,
        Dropped = 4,
        Failed = 5
    }
}