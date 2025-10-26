using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class CourseDto
    {
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
        public string? Status { get; set; } = CourseStatus.Pending; // Course approval status: Pending, Approved, Rejected, Draft
        public List<DepartmentDto> Departments { get; set; } = new();
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
        
        // Approval-related properties
        public string? ApprovalNote { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }
        
        // Computed properties
        public string StatusDisplay => IsActive ? "Hoạt động" : "Không hoạt động";
        public string DurationDisplay => Duration > 0 ? $"{Duration} giờ" : "Chưa xác định";
        public string DepartmentsDisplay => Departments.Any() ? string.Join(", ", Departments.Select(d => d.DepartmentName)) : "Tất cả phòng ban";
        public string ApprovalStatusDisplay => CourseStatus.GetDisplayText(Status ?? CourseStatus.Pending);
    }

    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class CourseDetailDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? CategoryId { get; set; }
        public string? Prerequisites { get; set; }
        public string? Objectives { get; set; }
        public decimal? Price { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class GetCoursesByIdentifiersRequest
    {
        public List<string> Identifiers { get; set; } = new List<string>();
    }

    public class CourseApprovalRequest
    {
        public int CourseId { get; set; }
        public string Action { get; set; } = string.Empty; // "approve" or "reject"
        public string? Reason { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }

    public class CourseApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }

    public class ApprovalHistoryDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Action { get; set; } = string.Empty; // Created, Submitted, Approved, Rejected
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string ActionBy { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        public string ActionIcon => ApprovalAction.GetIcon(Action);
        public string ActionColor => ApprovalAction.GetColor(Action);
    }
}
