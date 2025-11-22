using InternalTrainingSystem.WebApp.Constants;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class ClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;
        public string MentorName { get; set; } = string.Empty;
        public int MaxStudents { get; set; } = 30; // Số lượng sinh viên tối đa
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }  = ClassConstants.Status.Scheduled;
    }

    public class ClassDetailDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;
        public string MentorName { get; set; } = string.Empty;
        public List<ClassEmployeeDto> Employees { get; set; } = new();
    }

    public class ClassEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int AbsentNumberDay {  get; set; }
        public double ScoreFinal { get; set; }
    }

    public class ClassListDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;
        public string? MentorName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Status { get; set; }
    }

    public class CreateClassDto
    {
        public int CourseId { get; set; }
        public string MentorId { get; set; } = string.Empty;
        public List<string> StaffIds { get; set; } = new();
    }

    public class CreateClassRequest
    {
        public int CourseId { get; set; }
        public string MentorId { get; set; } = string.Empty;
        public List<string> StaffIds { get; set; } = new();
    }

    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int CourseId { get; set; }
        
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
        
        // Các thuộc tính chỉ dùng cho UI (không gửi về backend)
        public string CourseName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int NumberOfClasses { get; set; } = 1; // Số lượng lớp muốn tạo (chỉ dùng ở FE)
    }

    public class ClassScheduleItem
    {
        [Required(ErrorMessage = "Vui lòng chọn ngày học")]
        [DataType(DataType.Date)]
        public DateTime? ClassDate { get; set; }

        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public string? Room { get; set; }
    }

    public class ScheduleItemResponseDto
    {
        public int ScheduleId { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? MentorId { get; set; }
        public string Mentor { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public string? OnlineLink { get; set; }
        public string? AttendanceStatus { get; set; }
    }
}
