namespace InternalTrainingSystem.WebApp.Models.Quiz
{
    public class QuizViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimit  { get; set; } // Thời gian làm bài (phút)
        public int QuestionCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        
        // Properties mới cho unified QuizViewModel
        public string Subject { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public double? BestScore { get; set; }
        public DateTime? LastAttempt { get; set; }
        public bool IsCompleted { get; set; }
        public string Status { get; set; } = "Available";
        
        // Properties for quiz execution
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        
        // Helper methods cho icon và color dựa trên subject
        public string GetSubjectIcon()
        {
            return Subject.ToLower() switch
            {
                "lập trình" => "fa-code",
                "cơ sở dữ liệu" => "fa-database", 
                "mạng máy tính" => "fa-network-wired",
                "bảo mật" => "fa-shield-alt",
                "thiết kế" => "fa-paint-brush",
                "quản lý dự án" => "fa-tasks",
                "ai/ml" => "fa-robot",
                "devops" => "fa-cogs",
                "mobile" => "fa-mobile-alt",
                "web development" => "fa-globe",
                _ => "fa-question-circle"
            };
        }
        
        public string GetSubjectColor()
        {
            return Subject.ToLower() switch
            {
                "lập trình" => "primary",
                "cơ sở dữ liệu" => "success",
                "mạng máy tính" => "info", 
                "bảo mật" => "danger",
                "thiết kế" => "warning",
                "quản lý dự án" => "secondary",
                "ai/ml" => "dark",
                "devops" => "primary",
                "mobile" => "success",
                "web development" => "info",
                _ => "light"
            };
        }
        
        public string GetStatusBadgeClass()
        {
            return Status.ToLower() switch
            {
                "completed" => "badge-success",
                "in-progress" => "badge-primary",
                "available" => "badge-info",
                "locked" => "badge-secondary",
                _ => "badge-light"
            };
        }
    }
}