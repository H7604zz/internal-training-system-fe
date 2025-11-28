namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho thông tin bài tập
    /// </summary>
    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public int ClassId { get; set; }
        public int? ScheduleId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime DueAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentMimeType { get; set; }
        public long? AttachmentSizeBytes { get; set; }
        public List<AssignmentSubmissionSummaryDto> Submissions { get; set; } = new();
    }

    /// <summary>
    /// Form tạo bài tập mới (Mentor)
    /// </summary>
    public class CreateAssignmentForm
    {
        public int ClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueAt { get; set; }
        public IFormFile? File { get; set; }
    }

    /// <summary>
    /// Form cập nhật bài tập (Mentor)
    /// </summary>
    public class UpdateAssignmentForm
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public IFormFile? AttachmentFile { get; set; }
        public bool RemoveAttachment { get; set; }
    }

    /// <summary>
    /// Form nộp bài của Staff
    /// </summary>
    public class SubmitAssignmentForm
    {
        public IFormFile File { get; set; } = null!;
    }

    /// <summary>
    /// DTO chấm điểm bài nộp
    /// </summary>
    public class GradeSubmissionDto
    {
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt bài nộp cho danh sách
    /// </summary>
    public class AssignmentSubmissionSummaryDto
    {
        public int SubmissionId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    /// <summary>
    /// DTO chi tiết bài nộp
    /// </summary>
    public class AssignmentSubmissionDetailDto
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public string? Note { get; set; }
        
        // File info
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileMimeType { get; set; }
        public long? FileSizeBytes { get; set; }
        
        // Grading info
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
        public string? GradedByName { get; set; }
    }
}
