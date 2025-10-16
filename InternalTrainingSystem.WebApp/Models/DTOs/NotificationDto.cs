namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class DirectManagerStats
    {
        public int TotalCourses { get; set; }
        public int PendingStaff { get; set; }
        public int InvitedStaff { get; set; }
        public int ConfirmedStaff { get; set; }
    }
}
