using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.DTOs
{
    /// <summary>
    /// DTO cho việc tạo khóa học mới với modules và lessons
    /// </summary>
    public class CreateFullCourseDto
    {
        [Required(ErrorMessage = "Mã khóa học là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khóa học không được vượt quá 50 ký tự.")]
        [Display(Name = "Mã khóa học")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên khóa học không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên khóa học")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh mục khóa học là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CourseCategoryId { get; set; }

        [Required(ErrorMessage = "Thời lượng là bắt buộc.")]
        [Range(1, 1000, ErrorMessage = "Thời lượng phải từ 1 đến 1000 giờ.")]
        [Display(Name = "Thời lượng (giờ)")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Cấp độ là bắt buộc.")]
        [Display(Name = "Cấp độ")]
        public string Level { get; set; } = "Beginner";

        [Display(Name = "Khóa học online")]
        public bool IsOnline { get; set; } = true;

        [Display(Name = "Khóa học bắt buộc")]
        public bool IsMandatory { get; set; } = false;

        [Range(0, 10, ErrorMessage = "Điểm đạt phải từ 0 đến 10.")]
        [Display(Name = "Điểm đạt tối thiểu")]
        public double? PassScore { get; set; }

        [Display(Name = "Phòng ban áp dụng")]
        public List<int> SelectedDepartmentIds { get; set; } = new();

        [Required(ErrorMessage = "Khóa học phải có ít nhất 1 module.")]
        [MinLength(1, ErrorMessage = "Khóa học phải có ít nhất 1 module.")]
        public List<CreateModuleDto> Modules { get; set; } = new();

        // Files upload cho lessons
        public List<IFormFile> LessonFiles { get; set; } = new();

        // Computed properties for display
        public string LevelDisplay => Level switch
        {
            "Beginner" => "Cơ bản",
            "Intermediate" => "Trung cấp",
            "Advanced" => "Nâng cao",
            _ => Level
        };

        public string TypeDisplay => IsOnline ? "Trực tuyến" : "Tại lớp";
        public string MandatoryDisplay => IsMandatory ? "Bắt buộc" : "Tự nguyện";
    }

    /// <summary>
    /// DTO cho module trong khóa học
    /// </summary>
    public class CreateModuleDto
    {
        [Required(ErrorMessage = "Tiêu đề module là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề module không được vượt quá 200 ký tự.")]
        [Display(Name = "Tiêu đề module")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả module không được vượt quá 1000 ký tự.")]
        [Display(Name = "Mô tả module")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự module phải >= 1.")]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Module phải có ít nhất 1 bài học.")]
        [MinLength(1, ErrorMessage = "Module phải có ít nhất 1 bài học.")]
        public List<CreateLessonDto> Lessons { get; set; } = new();
    }

    /// <summary>
    /// DTO cho lesson trong module (khớp với backend NewLessonSpecDto)
    /// </summary>
    public class CreateLessonDto
    {
        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề bài học không được vượt quá 200 ký tự.")]
        [Display(Name = "Tiêu đề bài học")]
        public string Title { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự bài học phải >= 1.")]
        [Display(Name = "Thứ tự")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Loại bài học là bắt buộc.")]
        [Display(Name = "Loại bài học")]
        public LessonType Type { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // Video: VideoUrl -> ContentUrl
        // Reading: không dùng
        [Display(Name = "URL nội dung")]
        public string? ContentUrl { get; set; }

        // Reading: MainFileIndex
        // Quiz: QuizFileIndex
        [Display(Name = "File index")]
        public int? MainFileIndex { get; set; }

        // Video: DocumentUrl -> AttachmentUrl
        // Reading: AdditionalDocumentUrl -> AttachmentUrl
        [Display(Name = "URL tài liệu đính kèm")]
        public string? AttachmentUrl { get; set; }

        // Quiz only
        [Display(Name = "Tiêu đề quiz")]
        public string? QuizTitle { get; set; }

        [Display(Name = "Quiz từ file Excel")]
        public bool IsQuizExcel { get; set; } = true; // Mặc định true vì quiz chỉ hỗ trợ Excel

        [Range(1, 600, ErrorMessage = "Thời gian làm bài từ 1-600 phút")]
        [Display(Name = "Thời gian làm bài (phút)")]
        public int? QuizTimeLimit { get; set; }

        [Range(1, 20, ErrorMessage = "Số lần làm bài từ 1-20")]
        [Display(Name = "Số lần làm bài tối đa")]
        public int? QuizMaxAttempts { get; set; }

        [Range(0, 100, ErrorMessage = "Điểm pass từ 0-100%")]
        [Display(Name = "Điểm pass (%)")]
        public int? QuizPassingScore { get; set; }

        // Computed properties for display
        public string TypeDisplay => Type switch
        {
            LessonType.Video => "Video",
            LessonType.Reading => "Bài Đọc",
            LessonType.Quiz => "Quiz",
            _ => Type.ToString()
        };
    }

    /// <summary>
    /// Enum cho loại bài học (khớp với backend)
    /// </summary>
    public enum LessonType
    {
        Video = 1,
        Reading = 2,
        Quiz = 3
    }

    /// <summary>
    /// DTO cho việc tạo khóa học đơn giản (không có modules/lessons)
    /// </summary>
    public class CreateSimpleCourseDto
    {
        [Required(ErrorMessage = "Mã khóa học là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khóa học không được vượt quá 50 ký tự.")]
        [Display(Name = "Mã khóa học")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên khóa học không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên khóa học")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh mục khóa học là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CourseCategoryId { get; set; }

        [Required(ErrorMessage = "Thời lượng là bắt buộc.")]
        [Range(1, 1000, ErrorMessage = "Thời lượng phải từ 1 đến 1000 giờ.")]
        [Display(Name = "Thời lượng (giờ)")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Cấp độ là bắt buộc.")]
        [Display(Name = "Cấp độ")]
        public string Level { get; set; } = "Beginner";

        [Display(Name = "Khóa học online")]
        public bool IsOnline { get; set; } = true;

        [Display(Name = "Khóa học bắt buộc")]
        public bool IsMandatory { get; set; } = false;

        [Display(Name = "Phòng ban áp dụng")]
        public List<int> SelectedDepartmentIds { get; set; } = new();

        // Computed properties for display
        public string LevelDisplay => Level switch
        {
            "Beginner" => "Cơ bản",
            "Intermediate" => "Trung cấp",
            "Advanced" => "Nâng cao",
            _ => Level
        };

        public string TypeDisplay => IsOnline ? "Trực tuyến" : "Tại lớp";
        public string MandatoryDisplay => IsMandatory ? "Bắt buộc" : "Tự nguyện";
    }

    /// <summary>
    /// DTO cho response khi tạo khóa học thành công
    /// </summary>
    public class CreateCourseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO cho việc gửi metadata và files đến API
    /// </summary>
    public class CreateCourseApiRequest
    {
        public string Metadata { get; set; } = string.Empty;
        public List<IFormFile> LessonFiles { get; set; } = new();
    }
}