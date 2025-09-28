namespace InternalTrainingSystem.WebApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedCourses { get; set; }
        public List<Course> RecentCourses { get; set; } = new List<Course>();
        public List<Enrollment> RecentEnrollments { get; set; } = new List<Enrollment>();
    }

    public class CourseListViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public CourseLevel? LevelFilter { get; set; }
        public CourseStatus? StatusFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class UserListViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public string? SearchTerm { get; set; }
        public string? DepartmentFilter { get; set; }
        public UserRole? RoleFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }
}