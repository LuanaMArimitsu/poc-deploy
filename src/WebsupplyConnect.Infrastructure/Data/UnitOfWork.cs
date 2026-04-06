using Microsoft.EntityFrameworkCore.Storage;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Infrastructure.Data
{
    internal class UnitOfWork(WebsupplyConnectDbContext dbContext) : IUnitOfWork, IDisposable
    {
        private readonly WebsupplyConnectDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public bool HasActiveTransaction => _transaction != null;

        public bool HasPendingChanges => _context.ChangeTracker.HasChanges();

        public void SaveChanges()
        {
            EnsureTransaction();
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await EnsureTransactionAsync();
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public void Commit()
        {
            if (_transaction == null)
                return;

            try
            {
                _transaction.Commit();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
                _context.ChangeTracker.Clear();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }

                _context.ChangeTracker.Clear();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        private void EnsureTransaction()
        {
            if (_transaction == null)
            {
                _transaction = _context.Database.BeginTransaction();
            }
        }

        private async Task EnsureTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _transaction != null)
                {
                    try { _transaction.Rollback(); } catch { }
                    _transaction.Dispose();
                    _transaction = null;
                }

                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
