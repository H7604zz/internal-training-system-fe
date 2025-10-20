using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Helpers
{
    public static class CourseLevelHelper
    {
        /// <summary>
        /// Validate if level is valid
        /// </summary>
        public static bool IsValidLevel(string level)
        {
            if (string.IsNullOrEmpty(level))
                return false;
                
            var levels = CourseConstants.Levels.GetAllLevels();
            return levels.ContainsKey(level) || levels.ContainsValue(level);
        }
        
        /// <summary>
        /// Normalize level to constant value
        /// </summary>
        public static string NormalizeLevel(string level)
        {
            if (string.IsNullOrEmpty(level))
                return CourseConstants.Levels.Beginner;
                
            return CourseConstants.Levels.GetLevelValue(level);
        }
        
        /// <summary>
        /// Get select list for dropdowns
        /// </summary>
        public static List<SelectListItem> GetLevelSelectList(string selectedValue = null)
        {
            var levels = CourseConstants.Levels.GetAllLevels();
            var selectList = new List<SelectListItem>();
            
            foreach (var level in levels)
            {
                selectList.Add(new SelectListItem
                {
                    Value = level.Key,
                    Text = level.Value,
                    Selected = level.Key == selectedValue || level.Value == selectedValue
                });
            }
            
            return selectList;
        }
    }
    
    // Helper class for select list items if not available
    public class SelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; } = false;
    }
}