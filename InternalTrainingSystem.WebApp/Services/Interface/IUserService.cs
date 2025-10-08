using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IUserService
    {
        Task<List<EligibleStaffResponse>> GetUserRoleEligibleStaff(int courseId);
        Task<List<StaffConfirmCourseResponse>> GetUserRoleStaffConfirmCourse(int courseId);
    }
}
