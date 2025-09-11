namespace KontrolaPakowania.Server.Data
{
    public class UserSession
    {
        public string Username { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

        public void Login(string username)
        {
            Username = username;
        }

        public void Logout()
        {
            Username = null;
        }
    }
}