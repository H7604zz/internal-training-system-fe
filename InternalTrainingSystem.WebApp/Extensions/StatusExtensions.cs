using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Extensions
{
    /// <summary>
    /// Extension methods for status comparison
    /// </summary>
    public static class StatusExtensions
    {
        /// <summary>
        /// Check if course status is approved
        /// </summary>
        public static bool IsApproved(this string? status)
        {
            return string.Equals(status, CourseStatus.Approved, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if course status is pending
        /// </summary>
        public static bool IsPending(this string? status)
        {
            return string.Equals(status, CourseStatus.Pending, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if course status is rejected
        /// </summary>
        public static bool IsRejected(this string? status)
        {
            return string.Equals(status, CourseStatus.Rejected, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if course status is draft
        /// </summary>
        public static bool IsDraft(this string? status)
        {
            return string.Equals(status, CourseStatus.Draft, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if employee response is accepted
        /// </summary>
        public static bool IsAccepted(this string? responseType)
        {
            return string.Equals(responseType, EmployeeResponseType.Accepted, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if employee response is declined
        /// </summary>
        public static bool IsDeclined(this string? responseType)
        {
            return string.Equals(responseType, EmployeeResponseType.Declined, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if employee response is pending
        /// </summary>
        public static bool IsResponsePending(this string? responseType)
        {
            return string.Equals(responseType, EmployeeResponseType.Pending, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if employee is not invited
        /// </summary>
        public static bool IsNotInvited(this string? responseType)
        {
            return string.Equals(responseType, EmployeeResponseType.NotInvited, StringComparison.OrdinalIgnoreCase);
        }
    }
}