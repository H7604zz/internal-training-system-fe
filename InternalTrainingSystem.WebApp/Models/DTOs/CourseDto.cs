namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class CourseDto
    {
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
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int? MaxParticipants { get; set; }
        public int? CurrentParticipants { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new();
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
        
        // Computed properties
        public string StatusDisplay => IsActive ? "Hoạt động" : "Không hoạt động";
        public string DurationDisplay => Duration > 0 ? $"{Duration} giờ" : "Chưa xác định";
        public string DepartmentsDisplay => Departments.Any() ? string.Join(", ", Departments.Select(d => d.Name)) : "Tất cả phòng ban";
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
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
}
