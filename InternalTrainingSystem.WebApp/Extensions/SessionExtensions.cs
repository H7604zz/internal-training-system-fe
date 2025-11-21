using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Extensions
{
    public static class SessionExtensions
    {
        public static void SetUserInfo(this ISession session, string fullName, string email, string id, string role)
        {
            session.SetString("UserFullName", fullName);
            session.SetString("Name", fullName); // Alias for compatibility
            session.SetString("UserEmail", email);
            session.SetString("Email", email); // Alias for compatibility
            session.SetString("Id", id);
            session.SetString("Role", role);
        }

        public static void ClearUserInfo(this ISession session)
        {
            session.Remove("UserFullName");
            session.Remove("Name");
            session.Remove("UserEmail");
            session.Remove("Email");
            session.Remove("Id");
            session.Remove("Role");
        }
    }
}