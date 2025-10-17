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
        public List<ClassStudentDto> Students { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int MaxStudents { get; set; } = 30; // Số lượng sinh viên tối đa
        public string CreatedBy { get; set; } = string.Empty; // Người tạo lớp
        
        // Computed properties
        public int CurrentStudentCount => Students?.Count ?? 0;
        public string Status => IsActive ? "Hoạt động" : "Không hoạt động";
        public string StudentCountDisplay => $"{CurrentStudentCount}/{MaxStudents}";
    }

    public class ClassStudentDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
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
