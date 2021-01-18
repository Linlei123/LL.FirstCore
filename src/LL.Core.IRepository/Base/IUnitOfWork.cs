using LL.Core.EFCore.Context;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace LL.Core.IRepository.Base
{
    public interface IUnitOfWork
    {
        BaseDbContext DbContext { get; }
        void Begin(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        void Commit();
        void Rollback();
        int Save();

        Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveAsync();
    }
}
