using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.WebApp.Extensions
{
    public static class SessionExtensions
    {
        public static void SetUserInfo(this ISession session, string fullName, string email, string id)
        {
            session.SetString("UserFullName", fullName);
            session.SetString("UserEmail", email);
            session.SetString("Id", id);
        }

        public static void ClearUserInfo(this ISession session)
        {
            session.Remove("UserFullName");
            session.Remove("UserEmail");
            session.Remove("Id");
        }
    }
}