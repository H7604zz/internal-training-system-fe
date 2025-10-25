namespace InternalTrainingSystem.WebApp.Constants
{
    /// <summary>
    /// Constants for Course Status
    /// </summary>
    public static class CourseStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Draft = "Draft";

        /// <summary>
        /// Get all available course statuses
        /// </summary>
        public static readonly string[] AllStatuses = { Pending, Approved, Rejected, Draft };

        /// <summary>
        /// Get display text for course status
        /// </summary>
        public static string GetDisplayText(string status)
        {
            return status switch
            {
                Pending => "Chờ phê duyệt",
                Approved => "Đã phê duyệt",
                Rejected => "Đã từ chối",
                Draft => "Bản nháp",
                _ => "Không xác định"
            };
        }
    }

    /// <summary>
    /// Constants for Employee Status
    /// </summary>
    public static class EmployeeStatus
    {
        public const string NotEnrolled = "NotEnrolled"; // chưa tham gia
        public const string Enrolled = "Enrolled"; // đồng ý tham gia
        public const string InProgress = "InProgress"; // trong quá trình học
        public const string Completed = "Completed"; // hoàn thành

        /// <summary>
        /// Get all available employee statuses
        /// </summary>
        public static readonly string[] AllStatuses = { NotEnrolled, Enrolled, InProgress, Completed };

        /// <summary>
        /// Get display text for employee status
        /// </summary>
        public static string GetDisplayText(string status)
        {
            return status switch
            {
                NotEnrolled => "Chưa tham gia",
                Enrolled => "Đồng ý tham gia", 
                InProgress => "Trong quá trình học",
                Completed => "Hoàn thành",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Get icon class for employee status
        /// </summary>
        public static string GetIcon(string status)
        {
            return status switch
            {
                NotEnrolled => "fa-circle",
                Enrolled => "fa-check-circle",
                InProgress => "fa-clock",
                Completed => "fa-trophy",
                _ => "fa-question-circle"
            };
        }

        /// <summary>
        /// Get badge class for employee status
        /// </summary>
        public static string GetBadgeClass(string status)
        {
            return status switch
            {
                NotEnrolled => "badge-secondary",
                Enrolled => "badge-success",
                InProgress => "badge-warning",
                Completed => "badge-primary",
                _ => "badge-secondary"
            };
        }
    }

    /// <summary>
    /// Constants for Employee Response Type
    /// </summary>
    public static class EmployeeResponseType
    {
        public const string Accepted = "Accepted";
        public const string Declined = "Declined";
        public const string Pending = "Pending";
        public const string NotInvited = "NotInvited";

        /// <summary>
        /// Get all available response types
        /// </summary>
        public static readonly string[] AllResponseTypes = { Accepted, Declined, Pending, NotInvited };

        /// <summary>
        /// Get display text for response type
        /// </summary>
        public static string GetDisplayText(string responseType)
        {
            return responseType switch
            {
                Accepted => "Đồng ý tham gia",
                Declined => "Từ chối",
                Pending => "Chưa phản hồi",
                NotInvited => "Chưa được mời",
                _ => "Không xác định"
            };
        }
    }

    /// <summary>
    /// Constants for Approval Actions
    /// </summary>
    public static class ApprovalAction
    {
        public const string Created = "Created";
        public const string Submitted = "Submitted";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Approve = "approve";
        public const string Reject = "reject";

        /// <summary>
        /// Get all available approval actions
        /// </summary>
        public static readonly string[] AllActions = { Created, Submitted, Approved, Rejected };

        /// <summary>
        /// Get icon class for approval action
        /// </summary>
        public static string GetIcon(string action)
        {
            return action?.ToLower() switch
            {
                "created" => "fas fa-plus",
                "submitted" => "fas fa-paper-plane",
                "approved" => "fas fa-check-circle",
                "rejected" => "fas fa-times-circle",
                _ => "fas fa-info-circle"
            };
        }

        /// <summary>
        /// Get color class for approval action
        /// </summary>
        public static string GetColor(string action)
        {
            return action?.ToLower() switch
            {
                "created" => "created",
                "submitted" => "pending",
                "approved" => "approved",
                "rejected" => "rejected",
                _ => "default"
            };
        }
    }

    /// <summary>
    /// Constants for Error Codes
    /// </summary>
    public static class ErrorCode
    {
        public const string InvalidRequest = "INVALID_REQUEST";
        public const string CourseNotFound = "COURSE_NOT_FOUND";
        public const string InvalidAction = "INVALID_ACTION";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ReasonTooShort = "REASON_TOO_SHORT";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
    }
}