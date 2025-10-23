using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface INotificationService
    {
        Task<NotificationResponse> NotifyEligibleUsersAsync(int courseId);
        Task<(bool IsSent, DateTime? SentAt)> CheckNotificationStatusAsync(int courseId, NotificationType type);
        Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10);
        Task<ServiceResult> MarkNotificationAsReadAsync(int notificationId, string userId);
        Task<ServiceResult> MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
    }
}
