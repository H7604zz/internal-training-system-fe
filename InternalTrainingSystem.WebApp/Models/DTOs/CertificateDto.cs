namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho chứng chỉ hoàn thành khóa học (khớp với CertificateResponse từ API)
    /// </summary>
    public class CertificateDto
    {
        public int CertificateId { get; set; }
        public string CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CertificateName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        
        // Computed properties
        public string IssueDateDisplay => IssueDate.ToString("dd/MM/yyyy");
    }
}
