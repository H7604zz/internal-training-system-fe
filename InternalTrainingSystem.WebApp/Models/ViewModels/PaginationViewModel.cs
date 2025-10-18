using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.WebApp.Models.ViewModels
{
    /// <summary>
    /// View model for pagination component
    /// </summary>
    public class PaginationViewModel
    {
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Trang hiện tại phải lớn hơn 0")]
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Total number of pages
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Tổng số trang phải lớn hơn hoặc bằng 0")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Số item mỗi trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Action name for pagination links
        /// </summary>
        [Required]
        public string ActionName { get; set; } = "Index";

        /// <summary>
        /// Controller name for pagination links
        /// </summary>
        public string ControllerName { get; set; } = "";

        /// <summary>
        /// Additional route values for pagination links (search parameters, filters, etc.)
        /// </summary>
        public Dictionary<string, object>? RouteValues { get; set; }

        /// <summary>
        /// Aria label for accessibility
        /// </summary>
        public string AriaLabel { get; set; } = "Phân trang";

        /// <summary>
        /// Total number of items (optional, for display purposes)
        /// </summary>
        public int? TotalItems { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PaginationViewModel()
        {
            RouteValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="currentPage">Current page number</param>
        /// <param name="totalPages">Total number of pages</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Additional route values</param>
        public PaginationViewModel(int currentPage, int totalPages, int pageSize, 
            string actionName = "Index", string controllerName = "", 
            Dictionary<string, object>? routeValues = null)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
            ActionName = actionName;
            ControllerName = controllerName;
            RouteValues = routeValues ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Check if pagination should be displayed
        /// </summary>
        public bool ShouldShowPagination => TotalPages > 1;

        /// <summary>
        /// Check if current page has previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Check if current page has next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Get previous page number
        /// </summary>
        public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;

        /// <summary>
        /// Get next page number
        /// </summary>
        public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;

        /// <summary>
        /// Get display text for current page info
        /// </summary>
        public string GetPageInfo()
        {
            if (TotalItems.HasValue)
            {
                int startItem = (CurrentPage - 1) * PageSize + 1;
                int endItem = Math.Min(CurrentPage * PageSize, TotalItems.Value);
                return $"Hiển thị {startItem}-{endItem} trong tổng số {TotalItems.Value} kết quả";
            }
            return $"Trang {CurrentPage} / {TotalPages}";
        }
    }
}