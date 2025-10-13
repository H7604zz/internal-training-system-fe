using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Models.ViewModels
{
    public class CreateClassViewModel
    {
        public List<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public List<MentorResponse> Mentors { get; set; } = new List<MentorResponse>();
    }
}
