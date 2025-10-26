using InternalTrainingSystem.WebApp.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.ViewModels
{
    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Tên lớp học là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lớp học không được vượt quá 100 ký tự")]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mentor")]
        public int MentorId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);

        [Required(ErrorMessage = "Sức chứa lớp học là bắt buộc")]
        [Range(1, 100, ErrorMessage = "Sức chứa phải từ 1 đến 100 học viên")]
        public int Capacity { get; set; } = 20;

        public int CreateById { get; set; }

        // Schedule information
        public List<ClassScheduleItem> Schedule { get; set; } = new List<ClassScheduleItem>();

        // Dropdown data
        public List<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public List<MentorResponse> Mentors { get; set; } = new List<MentorResponse>();
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
}
