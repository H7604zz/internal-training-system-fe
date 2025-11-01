using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Extensions;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho khóa học của nhân viên (match với API CourseListItemDto)
    /// </summary>
    public class EmployeeCourseDto
    {
        // Properties từ API CourseListItemDto
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Status { get; set; } = string.Empty; // Enrollment status: NotEnrolled, Enrolled, InProgress, Completed, Dropped
        public List<DepartmnentViewDto> Departments { get; set; } = new();
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;

        // Computed properties cho view
        /// <summary>
        /// Course code display (alias cho Code)
        /// </summary>
        public string CourseCode => Code ?? "N/A";
        
        /// <summary>
        /// Duration display với đơn vị
        /// </summary>
        public string DurationDisplay => $"{Duration} giờ";
        
        /// <summary>
        /// Department names joined
        /// </summary>
        public string DepartmentName => string.Join(", ", Departments?.Select(d => d.DepartmentName) ?? new List<string>());
        
        /// <summary>
        /// Trainer name (alias cho CreatedBy)
        /// </summary>
        public string TrainerName => CreatedBy ?? "N/A";
        
        /// <summary>
        /// Response type (alias cho Status)
        /// </summary>
        public string ResponseType => Status ?? "NotEnrolled";
        
        /// <summary>
        /// Invited date (alias cho CreatedDate)
        /// </summary>
        public DateTime InvitedDate => CreatedDate;
        
        // Mock data properties (có thể bổ sung sau)
        //public DateTime StartDate { get; set; } = DateTime.Now.AddDays(30);
        //public DateTime EndDate { get; set; } = DateTime.Now.AddDays(60);
        public DateTime? ResponseDate { get; set; }
        public string? Note { get; set; }
        public string CourseStatus { get; set; } = "Open";
        
        /// <summary>
        /// Hiển thị thời gian phản hồi formatted
        /// </summary>
        public string ResponseDateDisplay => ResponseDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
        
        /// <summary>
        /// Hiển thị thời gian bắt đầu formatted
        /// </summary>
        //public string StartDateDisplay => StartDate.ToString("dd/MM/yyyy");
        
        /// <summary>
        /// Hiển thị thời gian kết thúc formatted
        /// </summary>
        //public string EndDateDisplay => EndDate.ToString("dd/MM/yyyy");
        
        /// <summary>
        /// Tính số ngày còn lại để phản hồi (nếu còn pending)
        /// </summary>
        //public int DaysLeftToRespond => ResponseType == "NotEnrolled" ? Math.Max(0, (StartDate - DateTime.Now).Days) : 0;
        
        /// <summary>
        /// Kiểm tra có phải đang chờ phản hồi không
        /// </summary>
        public bool IsPending => ResponseType == "NotEnrolled";
        
        /// <summary>
        /// Kiểm tra đã tham gia hay chưa
        /// </summary>
        public bool IsAccepted => ResponseType == "Enrolled";
        
        /// <summary>
        /// Kiểm tra đã từ chối hay chưa
        /// </summary>
        public bool IsDeclined => ResponseType == "Dropped";
        
        /// <summary>
        /// Kiểm tra đang trong quá trình học hay không
        /// </summary>
        public bool IsInProgress => ResponseType == "InProgress";
        
        /// <summary>
        /// Kiểm tra đã hoàn thành hay chưa
        /// </summary>
        public bool IsCompleted => ResponseType == "Completed";
        
        /// <summary>
        /// Hiển thị badge class cho trạng thái
        /// </summary>
        public string StatusBadgeClass => ResponseType.ToLower() switch
        {
            "enrolled" => "badge-success",
            "inprogress" => "badge-info",
            "completed" => "badge-primary",
            "dropped" => "badge-danger",
            "notenrolled" => "badge-warning",
            _ => "badge-secondary"
        };
        
        /// <summary>
        /// Hiển thị icon cho trạng thái
        /// </summary>
        public string StatusIcon => ResponseType.ToLower() switch
        {
            "enrolled" => "fa-check-circle",
            "inprogress" => "fa-play-circle",
            "completed" => "fa-trophy",
            "dropped" => "fa-times-circle",
            "notenrolled" => "fa-clock",
            _ => "fa-question-circle"
        };
        
        /// <summary>
        /// Hiển thị text cho trạng thái
        /// </summary>
        public string StatusText => ResponseType.ToLower() switch
        {
            "enrolled" => "Đã tham gia",
            "inprogress" => "Đang học",
            "completed" => "Đã hoàn thành",
            "dropped" => "Đã từ chối",
            "notenrolled" => "Chờ phản hồi",
            _ => "N/A"
        };
        
        /// <summary>
        /// Hiển thị badge class cho trạng thái khóa học
        /// </summary>
        public string CourseStatusBadgeClass => CourseStatus.ToLower() switch
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
        public string CourseStatusText => CourseStatus.ToLower() switch
        {
            "open" => "Đang mở",
            "inprogress" => "Đang diễn ra",
            "completed" => "Đã hoàn thành",
            "cancelled" => "Đã hủy",
            _ => "N/A"
        };
        
        /// <summary>
        /// Hiển thị tên cấp độ
        /// </summary>
        public string LevelDisplay => Level.GetLevelDisplay();
        
        /// <summary>
        /// Hiển thị badge class cho cấp độ
        /// </summary>
        public string LevelBadgeClass => Level.GetLevelBadgeClass();
    }

    /// <summary>
    /// DTO cho request phản hồi khóa học
    /// </summary>
    public class CourseResponseRequestDto
    {
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public string ResponseType { get; set; } = string.Empty; // NotEnrolled, Enrolled, InProgress, Completed, Dropped
        
        public string? Note { get; set; }
    }
}