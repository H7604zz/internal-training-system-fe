using InternalTrainingSystem.WebApp.Constants;

namespace InternalTrainingSystem.WebApp.Extensions
{
    public static class CourseLevelExtensions
    {
        /// <summary>
        /// Get display name for course level
        /// </summary>
        public static string GetLevelDisplay(this string level)
        {
            return CourseConstants.Levels.GetDisplayName(level);
        }
        
        /// <summary>
        /// Get CSS badge class for course level
        /// </summary>
        public static string GetLevelBadgeClass(this string level)
        {
            return CourseConstants.Levels.GetBadgeClass(level);
        }
        
        /// <summary>
        /// Check if level is beginner
        /// </summary>
        public static bool IsBeginner(this string level)
        {
            var levelValue = CourseConstants.Levels.GetLevelValue(level);
            return levelValue == CourseConstants.Levels.Beginner;
        }
        
        /// <summary>
        /// Check if level is intermediate
        /// </summary>
        public static bool IsIntermediate(this string level)
        {
            var levelValue = CourseConstants.Levels.GetLevelValue(level);
            return levelValue == CourseConstants.Levels.Intermediate;
        }
        
        /// <summary>
        /// Check if level is advanced
        /// </summary>
        public static bool IsAdvanced(this string level)
        {
            var levelValue = CourseConstants.Levels.GetLevelValue(level);
            return levelValue == CourseConstants.Levels.Advanced;
        }
        
        /// <summary>
        /// Get level priority for sorting (1=Beginner, 2=Intermediate, 3=Advanced)
        /// </summary>
        public static int GetLevelPriority(this string level)
        {
            var levelValue = CourseConstants.Levels.GetLevelValue(level);
            return levelValue switch
            {
                CourseConstants.Levels.Beginner => 1,
                CourseConstants.Levels.Intermediate => 2,
                CourseConstants.Levels.Advanced => 3,
                _ => 1
            };
        }
       
    }
}