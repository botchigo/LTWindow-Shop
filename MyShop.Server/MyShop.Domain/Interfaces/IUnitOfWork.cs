namespace MyShop.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }        
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }
        IProductImageRepository ProductImages { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
