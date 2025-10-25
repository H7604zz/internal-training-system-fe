using InternalTrainingSystem.WebApp.Models;
using InternalTrainingSystem.WebApp.Models.DTOs;

namespace InternalTrainingSystem.WebApp.Services.Interface
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetCoursesByIdentifiersAsync(List<string> identifiers);
        Task<CourseDto?> GetCourseByIdAsync(int courseId);
        Task<PagedResult<CourseDto>> GetCoursesAsync(string? search = null, string? status = null, int page = 1, int pageSize = 10);
        Task<CourseDto?> CreateCourseAsync(CourseDto course);
        Task<CourseDto?> UpdateCourseAsync(CourseDto course);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<CourseApprovalResponse> ApproveCourseAsync(CourseApprovalRequest request);
        Task<CourseApprovalResponse> RejectCourseAsync(CourseApprovalRequest request);
        Task<List<CourseDto>> GetPendingCoursesAsync();
        Task<List<EmployeeCourseDto>> GetEmployeeCoursesAsync(string employeeId);
        Task<bool> RespondToCourseAsync(int courseId, string employeeId, string responseType, string? note = null);
        Task<PagedResult<EligibleStaffDto>> GetEligibleStaffAsync(int courseId, int page = 1, int pageSize = 10, string? employeeId = null, string? status = null);
    }
}
