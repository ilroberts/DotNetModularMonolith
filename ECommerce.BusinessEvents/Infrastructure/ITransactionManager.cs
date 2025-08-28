using System;
using System.Threading.Tasks;

namespace ECommerce.BusinessEvents.Infrastructure
{
    public interface ITransactionManager : IAsyncDisposable
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}

