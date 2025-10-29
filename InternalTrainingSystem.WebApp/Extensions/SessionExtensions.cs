using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Extensions
{
    public static class ControllerExtensions
    {
        public static bool IsUserLoggedIn(this Controller controller)
        {
            var userFullName = controller.HttpContext.Session.GetString("UserFullName");
            return !string.IsNullOrEmpty(userFullName);
        }

        public static string? GetCurrentUserName(this Controller controller)
        {
            return controller.HttpContext.Session.GetString("UserFullName");
        }

        public static string? GetCurrentUserEmail(this Controller controller)
        {
            return controller.HttpContext.Session.GetString("UserEmail");
        }

        public static string? GetCurrentEmployeeId(this Controller controller)
        {
            return controller.HttpContext.Session.GetString("EmployeeId");
        }
    }

    public static class SessionExtensions
    {
        public static void SetUserInfo(this ISession session, string fullName, string email, string? employeeId = null)
        {
            session.SetString("UserFullName", fullName);
            session.SetString("UserEmail", email);
            if (!string.IsNullOrEmpty(employeeId))
            {
                session.SetString("EmployeeId", employeeId);
            }
        }

        public static void ClearUserInfo(this ISession session)
        {
            session.Remove("UserFullName");
            session.Remove("UserEmail");
            session.Remove("EmployeeId");
        }
    }
}