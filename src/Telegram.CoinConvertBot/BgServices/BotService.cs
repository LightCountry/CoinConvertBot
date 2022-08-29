using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.CoinConvertBot.BgServices.Base;
using Telegram.CoinConvertBot.BgServices.BotHandler;

namespace Telegram.CoinConvertBot.BgServices
{
    public class BotService : MyBackgroundService
    {
        private readonly ITelegramBotClient _client;
        private readonly IFreeSql _freeSql;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BotService(ITelegramBotClient client,
            IFreeSql freeSql,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _client = client;
            _freeSql = freeSql;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true,
            };
            UpdateHandlers.freeSql = _freeSql;
            UpdateHandlers.configuration = _configuration;
            UpdateHandlers.serviceScopeFactory = _serviceScopeFactory;
            _client.StartReceiving(updateHandler: UpdateHandlers.HandleUpdateAsync,
                   pollingErrorHandler: UpdateHandlers.PollingErrorHandler,
                   receiverOptions: receiverOptions,
                   cancellationToken: stoppingToken);
            return Task.CompletedTask;
        }
    }
}
