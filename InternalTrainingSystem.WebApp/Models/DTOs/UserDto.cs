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

    public class MentorResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }

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
