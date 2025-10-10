namespace InternalTrainingSystem.WebApp.Models.Quiz
{
    public class QuizSubmissionModel
    {
        public int QuizId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();
    }
}