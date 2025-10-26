using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool IsInvited { get; set; } = false;
        public bool HasConfirmed { get; set; } = false;
        public DateTime? InvitedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin profile người dùng chi tiết
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }



    /// <summary>
    /// DTO để cập nhật thông tin profile
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// DTO cho nhân viên xác nhận khóa học
    /// </summary>
    public class StaffConfirmCourseResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin mentor
    /// </summary>
    public class MentorResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin nhân viên cơ bản
    /// </summary>
    public class StaffResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }

    /// <summary>
    /// DTO cho phản hồi của nhân viên về khóa học
    /// </summary>
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty; // Accepted, Declined, Pending
        public DateTime? ResponseDate { get; set; }
        public string? Note { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        
        public string ResponseTypeDisplay => EmployeeResponseType.GetDisplayText(ResponseType);

        public string ResponseDateDisplay => ResponseDate?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa phản hồi";
    }
}
