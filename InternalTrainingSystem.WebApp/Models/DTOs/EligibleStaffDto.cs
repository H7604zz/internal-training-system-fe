using InternalTrainingSystem.WebApp.Constants;
using InternalTrainingSystem.WebApp.Extensions;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho nhân viên đủ điều kiện tham gia khóa học
    /// </summary>
    public class EligibleStaffDto
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái: NotEnrolled, Enrolled, InProgress, Completed
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Lý do hoặc ghi chú
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Hiển thị badge class cho trạng thái
        /// </summary>
        public string StatusBadgeClass => EnrollmentConstants.GetBadgeClass(Status);
        
        /// <summary>
        /// Hiển thị text cho trạng thái
        /// </summary>
        public string StatusText => EnrollmentConstants.GetDisplayText(Status);

        /// <summary>
        /// Kiểm tra đã đăng ký hay chưa (đồng ý tham gia)
        /// </summary>
        public bool IsEnrolled => Status.IsEnrolled();

        /// <summary>
        /// Search helper - check if employee matches search term
        /// </summary>
        public bool MatchesSearchTerm(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return true;

            var term = searchTerm.ToLower();
            return EmployeeId.ToLower().Contains(term) ||
                   FullName.ToLower().Contains(term) ||
                   Email.ToLower().Contains(term);
        }
    }

    /// <summary>
    /// DTO cho việc xác nhận lý do từ chối của nhân viên (gửi đến API)
    /// </summary>
    public class ConfirmEnrollmentReasonDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
    }

    /// <summary>
    /// DTO cho request xác nhận từ form (nhận từ view)
    /// </summary>
    public class ConfirmEnrollmentReasonRequestDto
    {
        public int CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
    }
}