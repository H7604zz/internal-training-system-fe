using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface INotificationService
    {
         Task<NotificationResponse> NotifyEligibleUsersAsync(int courseId);
         Task<(bool IsSent, DateTime? SentAt)> CheckNotificationStatusAsync(int courseId, NotificationType type);

        }
}
