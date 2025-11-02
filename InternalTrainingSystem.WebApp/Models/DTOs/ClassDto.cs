using InternalTrainingSystem.WebApp.Constants;

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
        public string CourseName { get; set; } = string.Empty;
        public string MentorName { get; set; } = string.Empty;
        public List<ClassEmployeeDto> Employees { get; set; } = new();
    }

    public class ClassEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateClassesDto
    {
        public List<CreateClassDto> Classes { get; set; } = new();
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
}
