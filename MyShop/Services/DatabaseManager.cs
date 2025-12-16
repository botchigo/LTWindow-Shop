using Database;
using Database.Repositories;
using System;

namespace MyShop.Services
{
    /// <summary>
    /// Service ð? qu?n l? database context và repositories
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        private readonly AppDbContext _context;
        private IUserRepository? _userRepository;
        private ICategoryRepository? _categoryRepository;
        private IProductRepository? _productRepository;

        public DatabaseManager()
        {
            // C?u h?nh connection string - thay ð?i theo thông tin server c?a b?n
            var connectionString = BuildConnectionString(
                host: "localhost",
                port: 5432,
                database: "WindowApp_MyShop",
                username: "postgres",
                password: "23120138"
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

        /// <summary>
        /// User Repository ð? thao tác v?i users
        /// </summary>
        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }

        public ICategoryRepository CategoryRepository
        {
            get
            {
                _categoryRepository ??= new CategoryRepository(_context);
                return _categoryRepository;
            }
        }

        public IProductRepository ProductRepository
        {
            get
            {
                _productRepository ??= new ProductRepository(_context);
                return _productRepository;
            }
        }

        /// <summary>
        /// L?y DbContext (n?u c?n truy c?p tr?c ti?p)
        /// </summary>
        public AppDbContext Context => _context;

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
