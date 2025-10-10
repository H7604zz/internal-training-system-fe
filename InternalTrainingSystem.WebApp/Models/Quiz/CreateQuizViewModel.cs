namespace InternalTrainingSystem.WebApp.Models.Quiz
{
    public class CreateQuizViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SelectedChapterId { get; set; }
        public int SelectedSessionId { get; set; }
        public int QuestionCount { get; set; } = 10;
        public int Duration { get; set; } = 60; // minutes
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
        public bool IsRandomQuestions { get; set; } = true;
        public List<ChapterViewModel> Chapters { get; set; } = new List<ChapterViewModel>();
        public List<SessionViewModel> Sessions { get; set; } = new List<SessionViewModel>();
    }

    public class ChapterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SessionViewModel> Sessions { get; set; } = new List<SessionViewModel>();
    }

    public class SessionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ChapterId { get; set; }
        public int QuestionCount { get; set; } // Số câu hỏi có sẵn trong session này
    }
}