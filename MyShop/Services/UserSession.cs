using Database.models;

namespace MyShop.Services
{
    public static class UserSession
    {
        public static AppUser? CurrentUser { get; private set; }

        public static int UserId => CurrentUser?.UserId ?? 0;
        public static string UserName => CurrentUser?.FullName ?? "Khách";
        public static bool IsLoggedIn => CurrentUser != null;

        public static void StartSession(AppUser user)
        {
            CurrentUser = user;
        }

        public static void StopSession()
        {
            CurrentUser = null;
        }
    }
}
