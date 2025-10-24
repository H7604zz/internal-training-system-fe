using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
    }
}
