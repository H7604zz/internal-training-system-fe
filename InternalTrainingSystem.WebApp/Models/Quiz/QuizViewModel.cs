namespace InternalTrainingSystem.WebApp.Models.Quiz
{
    public class QuizViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; } // Thời gian làm bài (phút)
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }
}