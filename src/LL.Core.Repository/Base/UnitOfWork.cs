using LL.Core.EFCore.Context;
using LL.Core.IRepository.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LL.Core.Repository.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private BaseDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public UnitOfWork(BaseDbContext baseDbContext)
        {
            _dbContext = baseDbContext;
        }

        public BaseDbContext DbContext { get { return _dbContext; } }
        public void Begin(IsolationLevel isolationLevel)
        {
            if (_transaction == null)
            {
                _transaction = _dbContext.Database.BeginTransaction(isolationLevel);
            }
        }

        public async Task BeginAsync(IsolationLevel isolationLevel)
        {
            if (_transaction == null)
            {
                _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel);
            }
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                Save();
                _transaction.Commit();
                Disposable();
            }
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await SaveAsync();
                await _transaction.CommitAsync();
                await DisposableAsync();
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                Disposable();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await DisposableAsync();
            }
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        private void Disposable()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
        private async Task DisposableAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
