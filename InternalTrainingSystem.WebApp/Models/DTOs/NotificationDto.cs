using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CourseId { get; set; }
        public string? UserId { get; set; }
        public bool IsRead { get; set; }
        public string? Link { get; set; }
        public string Title => GetTitleFromType();

        private string GetTitleFromType()
        {
            return Type switch
            {
                NotificationType.Start => "Khóa học bắt đầu",
                NotificationType.Reminder => "Nhắc nhở",
                NotificationType.Finish => "Hoàn thành khóa học",
                NotificationType.StaffConfirm => "Xác nhận nhân viên",
                NotificationType.Certificate => "Chứng chỉ được cấp",
                _ => "Thông báo"
            };
        }

        public string Icon => GetIconFromType();

        private string GetIconFromType()
        {
            return Type switch
            {
                NotificationType.Start => "fas fa-play-circle text-success",
                NotificationType.Reminder => "fas fa-bell text-warning",
                NotificationType.Finish => "fas fa-check-circle text-success",
                NotificationType.StaffConfirm => "fas fa-user-check text-info",
                NotificationType.Certificate => "fas fa-certificate text-warning",
                _ => "fas fa-info-circle text-secondary"
            };
        }
    }

    public class NotificationListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<NotificationDto> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
