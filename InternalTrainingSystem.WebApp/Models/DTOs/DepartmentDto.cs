using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO for list view - matches API's DepartmentListDto
    /// </summary>
    public class DepartmnentViewDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for detail view - matches API's DepartmentDetailDto
    /// API chỉ trả về: DepartmentId, DepartmentName, Description, UserDetail
    /// </summary>
    public class DepartmentDetailDto
    {
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }
        
        /// <summary>
        /// Danh sách nhân viên trong phòng ban
        /// UserProfileDto đã được định nghĩa trong UserDto.cs
        /// </summary>
        public List<UserProfileDto>? UserDetail { get; set; }
    }
        
    /// <summary>
    /// DTO for creating new department - matches API's DepartmentRequestDto
    /// </summary>
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên phòng ban không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
    }
    
    /// <summary>
    /// DTO for updating department - matches API's DepartmentRequestDto
    /// </summary>
    public class UpdateDepartmentDto
    {
        public int DepartmentId { get; set; }
        
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên phòng ban không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
    }
}