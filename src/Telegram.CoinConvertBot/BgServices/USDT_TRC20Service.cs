using Flurl;
using Flurl.Http;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.CoinConvertBot.BgServices.Base;
using Telegram.CoinConvertBot.BgServices.BotHandler;
using Telegram.CoinConvertBot.Domains.Tables;
using Telegram.CoinConvertBot.Extensions;
using Telegram.CoinConvertBot.Helper;
using Telegram.CoinConvertBot.Models;

namespace Telegram.CoinConvertBot.BgServices
{
    public class USDT_TRC20Service : BaseScheduledService
    {
        private readonly ILogger<USDT_TRC20Service> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;

        public USDT_TRC20Service(ILogger<USDT_TRC20Service> logger,
            IConfiguration configuration,
            ITelegramBotClient botClient,
            IServiceProvider serviceProvider) : base("USDT-TRC20记录检测", TimeSpan.FromSeconds(30), logger)
        {
            _logger = logger;
            _configuration = configuration;
            _botClient = botClient;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;
            var _myTronConfig = provider.GetRequiredService<IOptionsSnapshot<MyTronConfig>>();
            var _repository = provider.GetRequiredService<IBaseRepository<TokenRecord>>();
            IHostEnvironment hostEnvironment = provider.GetRequiredService<IHostEnvironment>();
            var _bindRepository = provider.GetRequiredService<IBaseRepository<TokenBind>>();

            var payMinTime = DateTime.Now.AddSeconds(-60 * 5);
            var payMaxTime = DateTime.Now;
            var addressArray = _configuration.GetSection("Address:USDT-TRC20").Get<string[]>();
            if (addressArray.Length == 0)
            {
                _logger.LogWarning("未配置USDT收款地址！");
                return;
            }
            foreach (var address in addressArray)
            {

                var data = new
                {
                    start = 0,
                    limit = 200,
                    direction = "in",
                    tokens = _myTronConfig.Value.USDTContractAddress,
                    relatedAddress = address,
                    toAddress = address,
                    start_timestamp = (long)payMinTime.ToUnixTimeStamp(),
                    end_timestamp = (long)payMaxTime.ToUnixTimeStamp()
                };
                var transfers = await _myTronConfig.Value.ApiHost
                    .AppendPathSegment("api/token_trc20/transfers")
                    .SetQueryParams(data)
                    .GetJsonAsync();
                if (transfers.total > 0)
                {
                    var list = (IList<dynamic>)transfers.token_transfers;
                    foreach (var item in list)
                    {
                        //收款地址相同，已确认的订单
                        if (item.to_address != address || item.finalResult != "SUCCESS") continue;
                        //实际支付金额
                        var amount = Convert.ToDecimal(item.quant) / 1_000_000;
                        var record = new TokenRecord
                        {
                            BlockTransactionId = item.transaction_id,
                            FromAddress = item.from_address,
                            ToAddress = item.to_address,
                            OriginalAmount = amount,
                            OriginalCurrency = Currency.USDT,
                            ConvertCurrency = Currency.TRX,
                            Status = Status.Pending,
                            ReceiveTime = ((long)item.block_ts).ToDateTime()
                        };
                        if (!await _repository.Where(x => x.BlockTransactionId == record.BlockTransactionId).AnyAsync())
                        {
                            await _repository.InsertAsync(record);
                            _logger.LogInformation("新USDT入账：{@data}", record);
                            var AdminUserId = _configuration.GetValue<long>("BotConfig:AdminUserId");
                            try
                            {
                                var viewUrl = $"https://nile.tronscan.org/#/transaction/{record.BlockTransactionId}";
                                if (hostEnvironment.IsProduction())
                                {
                                    viewUrl = $"https://tronscan.org/#/transaction/{record.BlockTransactionId}";
                                }
                                Bot.Types.ReplyMarkups.InlineKeyboardMarkup inlineKeyboard = new(
                                    new[]
                                    {
                                            new []
                                            {
                                                Bot.Types.ReplyMarkups.InlineKeyboardButton.WithUrl("查看区块",viewUrl),
                                            },
                                    });
                                var _rateRepository = provider.GetRequiredService<IBaseRepository<TokenRate>>();
                                var rate = await _rateRepository.Where(x => x.Currency == Currency.USDT && x.ConvertCurrency == Currency.TRX).FirstAsync(x => x.Rate);
                                var binds = await _bindRepository.Where(x => x.Currency == Currency.TRX && x.Address == record.FromAddress).ToListAsync();
                                if (binds.Count > 0)
                                {
                                    foreach (var bind in binds)
                                    {
                                        try
                                        {
                                            await _botClient.SendTextMessageAsync(bind.UserId, $@"<b>我们已经收到您的USDT</b>
金额：<b>{record.OriginalAmount:#.######} {record.OriginalCurrency}</b>
哈希：<code>{record.BlockTransactionId}</code>
时间：<b>{record.ReceiveTime:yyyy-MM-dd HH:mm:ss}</b>
地址：<code>{record.FromAddress}</code>
预估：<b>{record.OriginalAmount.USDT_To_TRX(rate, UpdateHandlers.FeeRate)} TRX</b>

您的兑换申请已进入队列，预计5分钟内转入您的账户！
", Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
                                        }
                                        catch (Exception e)
                                        {
                                            _logger.LogError(e, $"给用户发送通知失败！用户ID：{bind.UserId}");
                                        }
                                    }
                                }
                                if (AdminUserId > 0)
                                {
                                    await _botClient.SendTextMessageAsync(AdminUserId, $@"<b>USDT入账通知！({record.OriginalAmount:#.######} {record.OriginalCurrency})</b>

订单：<code>{record.BlockTransactionId}</code>
转入：<b>{record.OriginalAmount:#.######} {record.OriginalCurrency}</b>
来源：<code>{record.FromAddress}</code>
接收：<code>{record.ToAddress}</code>
预估：<b>{record.OriginalAmount.USDT_To_TRX(rate, UpdateHandlers.FeeRate)} TRX</b>
时间：<b>{record.ReceiveTime:yyyy-MM-dd HH:mm:ss}</b>
", Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineKeyboard);
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "发送TG通知失败！");
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("暂无支付记录");
                }
            }
        }
    }

}
