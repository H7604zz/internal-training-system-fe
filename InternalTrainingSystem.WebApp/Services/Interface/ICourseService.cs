using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllCoursesAsync();
        Task<List<CourseDto>> GetCoursesByIdentifiersAsync(List<string> identifiers);
    }
}
