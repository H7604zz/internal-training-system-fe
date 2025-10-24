using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
    }
}
