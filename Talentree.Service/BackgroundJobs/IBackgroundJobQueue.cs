using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talentree.Service.BackgroundJobs
{
    public interface IBackgroundJobQueue
    {
        void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem);
        Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
