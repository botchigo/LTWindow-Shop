using System;
using Database;
using Database.Repositories;

namespace MyShop.Services
{
   
    public class DatabaseManager : IDisposable
    {
        private readonly AppDbContext _context;
        private IUserRepository? _userRepository;

        public DatabaseManager()
        {
      
            var connectionString = BuildConnectionString(
                host: "localhost",
                port: 5432,
                database: "MyShop",
                username: "postgres",
                password: "12345"
            );

            _context = AppDbContextFactory.CreateDbContext(connectionString);
        }

        public DatabaseManager(string host, int port, string database, string username, string password)
        {
            var connectionString = BuildConnectionString(host, port, database, username, password);
            _context = AppDbContextFactory.CreateDbContext(connectionString);
        }

        private static string BuildConnectionString(string host, int port, string database, string username, string password)
        {
            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

       
        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }

        public AppDbContext Context => _context;

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
