﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Butterfly.Protocol;
using Microsoft.Extensions.Logging;

namespace Butterfly.Flow.InMemory
{
    public class InMemorySpanConsumer : ISpanConsumer
    {
        private readonly IBlockingQueue<Span> _blockingQueue;
        private readonly ILogger<InMemorySpanConsumer> _logger;

        public InMemorySpanConsumer(IBlockingQueue<Span> blockingQueue, ILogger<InMemorySpanConsumer> logger = null)
        {
            _blockingQueue = blockingQueue ?? throw new ArgumentNullException(nameof(blockingQueue));
            _logger = logger;
        }

        public async Task AcceptAsync(ISpanConsumerCallback callback, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }

            var cts = new CancellationTokenSource();

            foreach (var spans in _blockingQueue.DequeueEnumerable(cts.Token))
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                        await callback.InvokeAsync(spans, cancellationToken);
                }
                catch (Exception)
                {
                    cts.Cancel(); 
                    throw;
                }
            }
        }
    }
}