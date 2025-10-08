using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IClassService
    {
        Task<List<ClassDto>> GetClassesAsync();
        Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto);
    }
}
