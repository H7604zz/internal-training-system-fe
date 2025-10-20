namespace InternalTrainingSystem.WebApp.Constants
{
    public static class CourseConstants
    {
        public static class Levels
        {
            public const string Beginner = "Beginner";
            public const string Intermediate = "Intermediate";
            public const string Advanced = "Advanced";
            
            // Vietnamese display names
            public const string BeginnerDisplay = "Cơ bản";
            public const string IntermediateDisplay = "Trung cấp";
            public const string AdvancedDisplay = "Nâng cao";
            
            /// <summary>
            /// Get display name for level
            /// </summary>
            public static string GetDisplayName(string level)
            {
                return level switch
                {
                    Beginner => BeginnerDisplay,
                    Intermediate => IntermediateDisplay,
                    Advanced => AdvancedDisplay,
                    BeginnerDisplay => BeginnerDisplay,
                    IntermediateDisplay => IntermediateDisplay,
                    AdvancedDisplay => AdvancedDisplay,
                    _ => level
                };
            }
            
            /// <summary>
            /// Get level value from display name
            /// </summary>
            public static string GetLevelValue(string displayName)
            {
                return displayName switch
                {
                    BeginnerDisplay => Beginner,
                    IntermediateDisplay => Intermediate,
                    AdvancedDisplay => Advanced,
                    Beginner => Beginner,
                    Intermediate => Intermediate,
                    Advanced => Advanced,
                    _ => Beginner
                };
            }
            
            /// <summary>
            /// Get CSS class for level badge
            /// </summary>
            public static string GetBadgeClass(string level)
            {
                var levelValue = GetLevelValue(level);
                return levelValue switch
                {
                    Beginner => "level-basic",
                    Intermediate => "level-intermediate",
                    Advanced => "level-advanced",
                    _ => "level-basic"
                };
            }
            
            /// <summary>
            /// Get all available levels for dropdown
            /// </summary>
            public static Dictionary<string, string> GetAllLevels()
            {
                return new Dictionary<string, string>
                {
                    { Beginner, BeginnerDisplay },
                    { Intermediate, IntermediateDisplay },
                    { Advanced, AdvancedDisplay }
                };
            }
        }

        public static class Status
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Draft = "Draft";
        }
    }

    public static class EnrollmentConstants
    {
        public static class Status
        {
            public const string NotEnrolled = "NotEnrolled";
            public const string Enrolled = "Enrolled";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Dropped = "Dropped";
        }
    }

    public static class QuizConstants
    {
        public static class Status
        {
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string TimedOut = "TimedOut";
        }

        public static class QuestionTypes
        {
            public const string MultipleChoice = "MultipleChoice";
            public const string TrueFalse = "TrueFalse";
            public const string Essay = "Essay";
        }
    }

    public static class ScheduleConstants
    {
        public static class Status
        {
            public const string Scheduled = "Scheduled";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
        }

        public static class ParticipantStatus
        {
            public const string Registered = "Registered";
            public const string Attended = "Attended";
            public const string NoShow = "NoShow";
            public const string Cancelled = "Cancelled";
        }
    }

    public static class CourseHistoryConstants
    {
        public static class Actions
        {
            public const string Enrolled = "Enrolled";
            public const string Started = "Started";
            public const string Paused = "Paused";
            public const string Resumed = "Resumed";
            public const string Completed = "Completed";
            public const string Dropped = "Dropped";
            public const string QuizStarted = "QuizStarted";
            public const string QuizCompleted = "QuizCompleted";
            public const string QuizPassed = "QuizPassed";
            public const string QuizFailed = "QuizFailed";
            public const string QuizRetaken = "QuizRetaken";
            public const string CertificateIssued = "CertificateIssued";
            public const string ScheduleRegistered = "ScheduleRegistered";
            public const string ScheduleAttended = "ScheduleAttended";
            public const string ScheduleCancelled = "ScheduleCancelled";
            public const string ProgressUpdated = "ProgressUpdated";
            public const string MaterialAccessed = "MaterialAccessed";
            public const string MaterialDownloaded = "MaterialDownloaded";
            public const string FeedbackSubmitted = "FeedbackSubmitted";
        }
    }

    public static class AttendanceConstants
    {
        public static class Status
        {
            public const string Present = "Present";
            public const string Absent = "Absent";
            public const string Excused = "Excused";
        }
    }

    public static class UserRoles
    {
        public const string Staff = "Staff"; // Nhân viên tham gia đào tạo
        public const string DirectManager = "DirectManager"; // Quản lý trực tiếp nhân viên
        public const string BoardOfDirectors = "BoardOfDirectors"; // Ban giám đốc
        public const string Administrator = "Administrator"; // Admin hệ thống
        public const string Mentor = "Mentor"; // Người hướng dẫn/giảng viên
        public const string TrainingDepartment = "TrainingDepartment"; // Phòng đào tạo
        public const string HR = "HR"; // Phòng nhân sự
        public const string System = "System"; // Hệ thống/AI chatbot
    }

    public enum NotificationType
    {
        Start = 1,
        Reminder = 2,
        Finish = 3,
        StaffConfirm = 4,
        Certificate = 5
    }
}
