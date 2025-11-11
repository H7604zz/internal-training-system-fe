namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho trang học tập của nhân viên
    /// </summary>
    public class CourseLearningDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Enrollment status
        public int Progress { get; set; } // 0-100
        public List<ModuleLearningDto> Modules { get; set; } = new();
        
        // Computed properties
        public string DurationDisplay => $"{Duration} giờ";
        public string ProgressDisplay => $"{Progress}%";
        public bool IsCompleted => Progress >= 100;
    }

    /// <summary>
    /// DTO cho module trong trang học tập
    /// </summary>
    public class ModuleLearningDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public List<LessonLearningDto> Lessons { get; set; } = new();
        public int CompletedLessons { get; set; }
        public int TotalLessons => Lessons.Count;
        public bool IsCompleted => CompletedLessons >= TotalLessons && TotalLessons > 0;
        public int Progress => TotalLessons > 0 ? (CompletedLessons * 100 / TotalLessons) : 0;
    }

    /// <summary>
    /// DTO cho bài học trong trang học tập
    /// </summary>
    public class LessonLearningDto
    {
        public int LessonId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty; // Video, Reading, Quiz
        public int OrderIndex { get; set; }
        public string? ContentUrl { get; set; }
        public string? AttachmentUrl { get; set; }  
        public int? QuizId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        // Quiz specific properties
        public QuizLearningDto? Quiz { get; set; }
        
        // Computed properties
        public string TypeDisplay => Type switch
        {
            "Video" => "Video",
            "Reading" => "Bài Đọc",
            "Quiz" => "Bài Kiểm Tra",
            _ => "Khác"
        };
        
        public string TypeIcon => Type switch
        {
            "Video" => "fa-video",
            "Reading" => "fa-book-open",
            "Quiz" => "fa-question-circle",
            _ => "fa-file"
        };
        
        public string StatusIcon => IsCompleted ? "fa-check-circle text-success" : "fa-circle text-muted";
    }

    /// <summary>
    /// DTO cho quiz trong trang học tập
    /// </summary>
    public class QuizLearningDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public int AttemptCount { get; set; }
        public int? BestScore { get; set; }
        public bool IsPassed { get; set; }
        public List<QuizAttemptDto> Attempts { get; set; } = new();
        
        // Computed properties
        public string TimeLimitDisplay => TimeLimit > 0 ? $"{TimeLimit} phút" : "Không giới hạn";
        public int RemainingAttempts => MaxAttempts - AttemptCount;
        public bool CanAttempt => MaxAttempts == 0 || AttemptCount < MaxAttempts;
        public string BestScoreDisplay => BestScore.HasValue ? $"{BestScore}%" : "Chưa làm bài";
        public string StatusDisplay => IsPassed ? "Đã hoàn thành" : (AttemptCount > 0 ? "Chưa hoàn thành" : "Chưa làm bài");
    }

    /// <summary>
    /// DTO cho lần làm bài quiz
    /// </summary>
    public class QuizAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Score { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        
        // Computed properties
        public string StartTimeDisplay => StartTime.ToString("dd/MM/yyyy HH:mm");
        public string EndTimeDisplay => EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa hoàn thành";
        public string ScoreDisplay => $"{Score}%";
        public string StatusDisplay => IsPassed ? "Đạt" : "Chưa đạt";
        public string StatusBadgeClass => IsPassed ? "badge-success" : "badge-danger";
        public string DurationDisplay
        {
            get
            {
                if (!EndTime.HasValue) return "Đang làm";
                var duration = EndTime.Value - StartTime;
                return $"{duration.TotalMinutes:F0} phút";
            }
        }
    }

    public class CompleteLessonRequest
    {
        public int LessonId { get; set; }
    }

    /// <summary>
    /// Response DTO for lesson progress update
    /// </summary>
    public class LessonProgressUpdateDto
    {
        public int CourseProgress { get; set; }
        public int ModuleProgress { get; set; }
        public bool IsCompleted { get; set; }
    }
}
