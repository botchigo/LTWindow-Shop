namespace MyShop.Core.Interfaces
{
    public interface IUserSessionService
    {
        bool IsLoggedIn { get; }
        int UserId { get; }
        string UserName { get; }
        string AccessToken { get; }

        void SetSession(string token);
        void ClearSession();
    }
}
