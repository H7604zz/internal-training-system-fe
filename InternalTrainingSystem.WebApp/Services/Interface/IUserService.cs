using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface IUserService
    {
        Task<List<EligibleStaffDto>> GetUserRoleEligibleStaff(int courseId);
        Task<List<StaffConfirmCourseResponse>> GetUserRoleStaffConfirmCourse(int courseId);
        Task<List<MentorResponse>> GetMentors();
        Task<List<StaffResponse>> GetAllStaff();
    }
}
