using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Telegram.CoinConvertBot.BgServices.Base
{
    public abstract class BaseScheduledService : IHostedService, IDisposable
    {
        private readonly Timer _timer;
        private readonly string jobName;
        private readonly TimeSpan _period;
        protected readonly ILogger Logger;

        protected BaseScheduledService(string JobName, TimeSpan period, ILogger logger)
        {
            Logger = logger;
            jobName = JobName;
            _period = period;
            _timer = new Timer(Execute, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Execute(object? state = null)
        {
            try
            {

                ExecuteAsync().Wait();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"定时任务[{jobName}]执行出现错误");
            }
            finally
            {
                _timer.Change(_period, Timeout.InfiniteTimeSpan);
            }
        }

        protected abstract Task ExecuteAsync();

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("任务{JobName}已启动。", jobName);
            _timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("任务{JobName}正在停止。", jobName);

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
