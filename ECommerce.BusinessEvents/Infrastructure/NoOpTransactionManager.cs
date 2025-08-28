using System;
using System.Threading.Tasks;

namespace ECommerce.BusinessEvents.Infrastructure
{
    public class NoOpTransactionManager : ITransactionManager
    {
        public Task BeginTransactionAsync() => Task.CompletedTask;
        public Task CommitAsync() => Task.CompletedTask;
        public Task RollbackAsync() => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

