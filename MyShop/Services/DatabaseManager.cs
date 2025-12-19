using Database;
using Database.Repositories;
using Microsoft.Extensions.Options;
using MyShop.Models;

namespace MyShop.Services
{
    public class DatabaseManager : IDisposable
    {
        private readonly AppDbContext _context;
        private IUserRepository? _userRepository;

        // Constructor cho DI
        public DatabaseManager(IOptions<DatabaseSettings> settings)
        {
            var connectionString = settings.Value.GetConnectionString();
            _context = AppDbContextFactory.CreateDbContext(connectionString);
        }

        // Constructor c? ð? backward compatibility (s? deprecated sau)
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
