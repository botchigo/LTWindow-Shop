namespace MyShop.Models
{
    public class DatabaseSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "MyShop";
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
        }
    }
}
