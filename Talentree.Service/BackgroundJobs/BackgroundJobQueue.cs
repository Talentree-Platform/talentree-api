using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Talentree.Service.BackgroundJobs
{
    public class BackgroundJobQueue : IBackgroundJobQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;

        public BackgroundJobQueue(int capacity = 10000)
        {
            // Bounded channel to prevent OutOfMemory issues under high load
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(options);
        }

        public void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _queue.Writer.TryWrite(workItem);
        }

        public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
