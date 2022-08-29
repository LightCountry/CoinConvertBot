using Microsoft.Extensions.Hosting;

namespace Telegram.CoinConvertBot.BgServices.Base
{
    public abstract class MyBackgroundService : IHostedService, IDisposable
    {
        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }

}
