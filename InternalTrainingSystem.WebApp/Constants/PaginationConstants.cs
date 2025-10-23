namespace InternalTrainingSystem.WebApp.Constants
{
    /// <summary>
    /// Constants cho việc phân trang (pagination) trong hệ thống
    /// </summary>
    public static class PaginationConstants
    {
        /// <summary>
        /// Số lượng khóa học hiển thị trên mỗi trang - dùng cho trang chính khóa học
        /// </summary>
        public const int CoursePageSize = 10;

        /// <summary>
        /// Số lượng lớp học hiển thị trên mỗi trang - dùng cho danh sách lớp học
        /// </summary>
        public const int ClassPageSize = 10;

        /// <summary>
        /// Số lượng khóa học của nhân viên hiển thị trên mỗi trang - dùng cho "Khóa học của tôi"
        /// </summary>
        public const int EmployeeCoursePageSize = 6;

        /// <summary>
        /// Số lượng nhân viên hiển thị trên mỗi trang - dùng cho danh sách nhân viên trong khóa học
        /// </summary>
        public const int EmployeePageSize = 12;

        /// <summary>
        /// Số lượng thông báo hiển thị trên mỗi trang
        /// </summary>
        public const int NotificationPageSize = 15;

        /// <summary>
        /// Số lượng bài quiz hiển thị trên mỗi trang
        /// </summary>
        public const int QuizPageSize = 8;
    }
}