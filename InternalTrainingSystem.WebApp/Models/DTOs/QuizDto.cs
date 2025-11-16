namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    public class QuizDetailDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public class StartQuizResponse
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TimeLimit { get; set; }
    }

    public class SubmitAttemptRequest
    {
        public List<SubmittedAnswerDto> Answers { get; set; } = new();
    }

    public class SubmittedAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new();
    }

    public class AttemptResultDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TimeLimit { get; set; }
        public int PassingScore { get; set; }
    }

    public class QuizInfoDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public int RemainingAttempts { get; set; }
        public bool IsLocked { get; set; }
        public bool HasPassed { get; set; }
        public decimal? BestScore { get; set; }
    }

    public class AttemptHistoryItem
    {
        public int AttemptId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Score { get; set; }
        public decimal? MaxScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
    }

    public class QuizForAttemptDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }
        public int AttemptId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }
}
