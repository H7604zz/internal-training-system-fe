namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho chứng chỉ hoàn thành khóa học (khớp với CertificateResponse từ API)
    /// </summary>
    public class CertificateDto
    {
        public int CertificateId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CertificateName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        // Computed properties
        public string IssueDateDisplay => IssueDate.ToString("dd/MM/yyyy");
        public string ExpirationDateDisplay => ExpirationDate?.ToString("dd/MM/yyyy") ?? "Không có";
        public bool HasExpiration => ExpirationDate.HasValue;
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.Now;
    }
}
