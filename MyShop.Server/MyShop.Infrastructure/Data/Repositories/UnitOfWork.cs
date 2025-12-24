using Database.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Interfaces;

namespace MyShop.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public IProductRepository Products { get; }

        public IProductImageRepository ProductImages { get; }

        public ICategoryRepository Categories { get; }

        public IOrderRepository Orders { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(AppDbContext context,
            IProductImageRepository productImageRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _context = context;

            ProductImages = productImageRepository;
            Categories = categoryRepository;
            Orders = orderRepository;
            Users = userRepository;
            Products = productRepository;
        }      

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction is not null)
                return;

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction is not null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("No active transaction to rollback.");

            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
