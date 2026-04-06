namespace WebsupplyConnect.Domain.Interfaces.Base
{
    public interface IUnitOfWork : IDisposable
    {
        bool HasActiveTransaction { get; }
        bool HasPendingChanges { get; }

        Task BeginTransactionAsync();
        void SaveChanges();
        Task SaveChangesAsync();

        void Commit();
        Task CommitAsync();

        void Rollback();
        Task RollbackAsync();
    }
}
