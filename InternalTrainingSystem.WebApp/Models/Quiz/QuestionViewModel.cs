namespace InternalTrainingSystem.WebApp.Models.Quiz
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswer { get; set; }
    }
}