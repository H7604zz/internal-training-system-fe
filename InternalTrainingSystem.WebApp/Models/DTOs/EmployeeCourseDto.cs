using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho khóa học của nhân viên
    /// </summary>
    public class EmployeeCourseDto
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái phản hồi: Pending, Accepted, Declined
        /// </summary>
        public string ResponseType { get; set; } = "Pending";
        
        /// <summary>
        /// Ngày phản hồi
        /// </summary>
        public DateTime? ResponseDate { get; set; }
        
        /// <summary>
        /// Ghi chú từ nhân viên
        /// </summary>
        public string? Note { get; set; }
        
        /// <summary>
        /// Ngày được mời tham gia
        /// </summary>
        public DateTime InvitedDate { get; set; }
        
        /// <summary>
        /// Độ ưu tiên: High, Medium, Low
        /// </summary>
        public string Priority { get; set; } = "Medium";
        
        /// <summary>
        /// Số lượng học viên tối đa
        /// </summary>
        public int MaxParticipants { get; set; }
        
        /// <summary>
        /// Số lượng học viên hiện tại
        /// </summary>
        public int CurrentParticipants { get; set; }
        
        /// <summary>
        /// Trạng thái khóa học: Open, InProgress, Completed, Cancelled
        /// </summary>
        public string Status { get; set; } = "Open";
        
        /// <summary>
        /// Hiển thị thời gian phản hồi formatted
        /// </summary>
        public string ResponseDateDisplay => ResponseDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
        
        /// <summary>
        /// Hiển thị thời gian bắt đầu formatted
        /// </summary>
        public string StartDateDisplay => StartDate.ToString("dd/MM/yyyy");
        
        /// <summary>
        /// Hiển thị thời gian kết thúc formatted
        /// </summary>
        public string EndDateDisplay => EndDate.ToString("dd/MM/yyyy");
        
        /// <summary>
        /// Hiển thị thời gian được mời formatted
        /// </summary>
        public string InvitedDateDisplay => InvitedDate.ToString("dd/MM/yyyy HH:mm");
        
        /// <summary>
        /// Tính số ngày còn lại để phản hồi (nếu còn pending)
        /// </summary>
        public int DaysLeftToRespond => ResponseType == "Pending" ? Math.Max(0, (StartDate - DateTime.Now).Days) : 0;
        
        /// <summary>
        /// Kiểm tra có phải đang chờ phản hồi không
        /// </summary>
        public bool IsPending => ResponseType == "Pending";
        
        /// <summary>
        /// Kiểm tra đã tham gia hay chưa
        /// </summary>
        public bool IsAccepted => ResponseType == "Accepted";
        
        /// <summary>
        /// Kiểm tra đã từ chối hay chưa
        /// </summary>
        public bool IsDeclined => ResponseType == "Declined";
        
        /// <summary>
        /// Hiển thị badge class cho trạng thái
        /// </summary>
        public string StatusBadgeClass => ResponseType.ToLower() switch
        {
            "accepted" => "badge-success",
            "declined" => "badge-danger",
            "pending" => "badge-warning",
            _ => "badge-secondary"
        };
        
        /// <summary>
        /// Hiển thị icon cho trạng thái
        /// </summary>
        public string StatusIcon => ResponseType.ToLower() switch
        {
            "accepted" => "fa-check-circle",
            "declined" => "fa-times-circle",
            "pending" => "fa-clock",
            _ => "fa-question-circle"
        };
        
        /// <summary>
        /// Hiển thị text cho trạng thái
        /// </summary>
        public string StatusText => ResponseType.ToLower() switch
        {
            "accepted" => "Đã tham gia",
            "declined" => "Đã từ chối",
            "pending" => "Chờ phản hồi",
            _ => "Không xác định"
        };
        
        /// <summary>
        /// Hiển thị badge class cho độ ưu tiên
        /// </summary>
        public string PriorityBadgeClass => Priority.ToLower() switch
        {
            "high" => "badge-danger",
            "medium" => "badge-warning",
            "low" => "badge-info",
            _ => "badge-secondary"
        };
        
        /// <summary>
        /// Hiển thị badge class cho trạng thái khóa học
        /// </summary>
        public string CourseStatusBadgeClass => Status.ToLower() switch
        {
            "open" => "badge-primary",
            "inprogress" => "badge-warning",
            "completed" => "badge-success",
            "cancelled" => "badge-danger",
            _ => "badge-secondary"
        };
        
        /// <summary>
        /// Hiển thị text cho trạng thái khóa học
        /// </summary>
        public string CourseStatusText => Status.ToLower() switch
        {
            "open" => "Đang mở",
            "inprogress" => "Đang diễn ra",
            "completed" => "Đã hoàn thành",
            "cancelled" => "Đã hủy",
            _ => "Không xác định"
        };
    }

    /// <summary>
    /// DTO cho request phản hồi khóa học
    /// </summary>
    public class CourseResponseRequestDto
    {
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public string ResponseType { get; set; } = string.Empty; // Accepted, Declined
        
        public string? Note { get; set; }
    }
}