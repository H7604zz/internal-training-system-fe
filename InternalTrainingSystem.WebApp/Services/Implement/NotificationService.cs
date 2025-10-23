using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Models.DTOs;
using InternalTrainingSystem.WebApp.Services.Interface;
using System.Text.Json;

namespace InternalTrainingSystem.WebApp.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;

        public NotificationService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<NotificationService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<(bool IsSent, DateTime? SentAt)> CheckNotificationStatusAsync(int courseId, NotificationType type)
        {
            bool isSent = false;
            DateTime? sentAt = null;

            try
            {
                var response = await _httpClient.GetAsync($"/api/course/{courseId}/notification-status/{(int)type}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("sent", out var sentProp) && sentProp.GetBoolean())
                    {
                        isSent = true;
                        if (root.TryGetProperty("sentAt", out var sentAtProp))
                            sentAt = sentAtProp.GetDateTime();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API kiểm tra trạng thái thông báo cho courseId {CourseId}", courseId);
            }

            return (isSent, sentAt);
        }

        public async Task<NotificationResponse> NotifyEligibleUsersAsync(int courseId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/notification/{courseId}/notify-eligible-users", null);

                if (!response.IsSuccessStatusCode)
                {
                    return new NotificationResponse
                    {
                        Success = false,
                        Message = "Gửi thông báo thất bại!"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<NotificationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null && !string.IsNullOrWhiteSpace(result.Message))
                {
                    result.Success = true;
                    return result;
                }

                return new NotificationResponse
                {
                    Success = false,
                    Message = "Gửi thông báo thất bại!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo cho courseId {CourseId}", courseId);
                return new NotificationResponse
                {
                    Success = false,
                    Message = "Gửi thông báo thất bại!"
                };
            }
        }

        public async Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notification/user/{userId}?page={page}&pageSize={pageSize}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<NotificationListResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result ?? new NotificationListResponse
                    {
                        Success = false,
                        Message = "Không thể lấy danh sách thông báo",
                        Notifications = new List<NotificationDto>()
                    };
                }

                return new NotificationListResponse
                {
                    Success = false,
                    Message = "Không thể lấy danh sách thông báo",
                    Notifications = new List<NotificationDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo cho userId {UserId}", userId);
                return new NotificationListResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách thông báo",
                    Notifications = new List<NotificationDto>()
                };
            }
        }

        public async Task<ServiceResult> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/notification/{notificationId}/mark-read?userId={userId}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Đã đánh dấu thông báo là đã đọc"
                    };
                }

                return new ServiceResult
                {
                    Success = false,
                    Message = "Không thể đánh dấu thông báo"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo {NotificationId} cho userId {UserId}", notificationId, userId);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đánh dấu thông báo"
                };
            }
        }

        public async Task<ServiceResult> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/notification/user/{userId}/mark-all-read", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Đã đánh dấu tất cả thông báo là đã đọc"
                    };
                }

                return new ServiceResult
                {
                    Success = false,
                    Message = "Không thể đánh dấu tất cả thông báo"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo cho userId {UserId}", userId);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đánh dấu thông báo"
                };
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notification/user/{userId}/unread-count");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("count", out var countProp))
                    {
                        return countProp.GetInt32();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số thông báo chưa đọc cho userId {UserId}", userId);
                return 0;
            }
        }
    }
}
