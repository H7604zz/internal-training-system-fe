using System.ComponentModel.DataAnnotations;

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
        public int? YearsOfExperience { get; set; }
        public DateTime? JoinDate { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<UserRoleHistoryDto> RoleHistory { get; set; } = new List<UserRoleHistoryDto>();
    }

    /// <summary>
    /// DTO cho lịch sử vai trò của người dùng
    /// </summary>
    public class UserRoleHistoryDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DurationText => EndDate.HasValue 
            ? $"{StartDate:dd/MM/yyyy} - {EndDate.Value:dd/MM/yyyy}"
            : $"{StartDate:dd/MM/yyyy} - Hiện tại";
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
    /// DTO cho danh sách nhân viên đủ điều kiện
    /// </summary>
    public class EligibleStaffResponse
    {
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
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
}
