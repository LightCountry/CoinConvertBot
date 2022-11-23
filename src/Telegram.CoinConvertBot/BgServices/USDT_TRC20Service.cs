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
using Telegram.CoinConvertBot.Domains.Tables;
using Telegram.CoinConvertBot.Extensions;
using Telegram.CoinConvertBot.Helper;
using Telegram.CoinConvertBot.Models;
using Telegram.CoinConvertBot.Models.TronModel;

namespace Telegram.CoinConvertBot.BgServices
{
    public class USDT_TRC20Service : BaseScheduledService
    {
        private readonly ILogger<USDT_TRC20Service> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITelegramBotClient _botClient;
        private readonly IHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;

        public USDT_TRC20Service(ILogger<USDT_TRC20Service> logger,
            IConfiguration configuration,
            ITelegramBotClient botClient,
            IHostEnvironment env,
            IServiceProvider serviceProvider) : base("USDT-TRC20记录检测", TimeSpan.FromSeconds(10), logger)
        {
            _logger = logger;
            _configuration = configuration;
            _botClient = botClient;
            _env = env;
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
            var addressArray = _configuration.GetSection("Address:USDT-TRC20").Get<string[]>();
            if (addressArray.Length == 0)
            {
                _logger.LogWarning("未配置USDT收款地址！");
                return;
            }
            var ContractAddress = _configuration.GetValue("TronConfig:USDTContractAddress", "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t");
            var BaseUrl = _configuration.GetValue("TronConfig:ApiHost", "https://api.trongrid.io");
            foreach (var address in addressArray)
            {
                var query = new Dictionary<string, object>();
                //query.Add("only_confirmed", true);
                query.Add("only_to", true);
                query.Add("limit", 50);
                query.Add("min_timestamp", (long)payMinTime.ToUnixTimeStamp());
                query.Add("contract_address", ContractAddress);
                var req = BaseUrl
                    .AppendPathSegment($"v1/accounts/{address}/transactions/trc20")
                    .SetQueryParams(query)
                    .WithTimeout(15);
                if (_env.IsProduction())
                    req = req.WithHeader("TRON-PRO-API-KEY", _configuration.GetValue("TronNet:ApiKey", ""));
                var result = await req
                    .GetJsonAsync<BaseResponse<Transactions>>();

                if (result.Success && result.Data?.Count > 0)
                {
                    foreach (var item in result.Data)
                    {
                        //合约地址不匹配
                        if (item.TokenInfo?.Address != ContractAddress) continue;
                        var types = new string[] { "Transfer", "TransferFrom" };
                        //收款地址相同
                        if (item.To != address || !types.Contains(item.Type)) continue;
                        //实际支付金额
                        var amount = item.Amount;
                        var record = new TokenRecord
                        {
                            BlockTransactionId = item.TransactionId,
                            FromAddress = item.From,
                            ToAddress = item.To,
                            //ContractAddress = item.TokenInfo.Address,
                            OriginalAmount = amount,
                            OriginalCurrency = Currency.USDT,
                            ConvertCurrency = Currency.TRX,
                            Status = Status.Pending,
                            ReceiveTime = item.BlockTimestamp.ToDateTime()
                        };
                        if (!await _repository.Where(x => x.BlockTransactionId == record.BlockTransactionId).AnyAsync())
                        {
                            await _repository.InsertAsync(record);
                            _logger.LogInformation("新{ConvertCurrency}入账：{@data}", record.ConvertCurrency, record);
                            var AdminUserId = _configuration.GetValue<long>("BotConfig:AdminUserId");
                            try
                            {
                                var viewUrl = $"https://shasta.tronscan.org/#/transaction/{record.BlockTransactionId}";
                                if (hostEnvironment.IsProduction())
                                {
                                    viewUrl = $"https://tronscan.org/#/transaction/{record.BlockTransactionId}";
                                }
                                Bot.Types.ReplyMarkups.InlineKeyboardMarkup inlineKeyboard = new(
                                    new[]
                                    {
                                            new []
                                            {
                                                Bot.Types.ReplyMarkups.InlineKeyboardButton.WithUrl("查看交易",viewUrl),
                                            },
                                    });

                                var binds = await _bindRepository.Where(x => x.Currency == Currency.TRX && x.Address == record.FromAddress).ToListAsync();
                                if (binds.Count > 0)
                                {
                                    foreach (var bind in binds)
                                    {
                                        try
                                        {
                                            await _botClient.SendTextMessageAsync(bind.UserId, $@"<b>我们已经收到您转出的{record.OriginalCurrency}</b>
金额：<b>{record.OriginalAmount:#.######} {record.OriginalCurrency}</b>
哈希：<code>{record.BlockTransactionId}</code>
时间：<b>{record.ReceiveTime:yyyy-MM-dd HH:mm:ss}</b>
地址：<code>{record.FromAddress}</code>

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
                                    var _rateRepository = provider.GetRequiredService<IBaseRepository<TokenRate>>();
                                    var rate = await _rateRepository.Where(x => x.Currency == Currency.USDT && x.ConvertCurrency == Currency.TRX).FirstAsync(x => x.Rate);
                                    await _botClient.SendTextMessageAsync(AdminUserId, $@"<b>{record.ConvertCurrency}入账通知！({record.OriginalAmount:#.######} {record.OriginalCurrency})</b>

订单：<code>{record.BlockTransactionId}</code>
原币：<b>{record.OriginalCurrency}</b>
转入：<b>{record.OriginalAmount:#.######} {record.OriginalCurrency}</b>
来源：<code>{record.FromAddress}</code>
接收：<code>{record.ToAddress}</code>
转换：<b>{record.ConvertCurrency}</b>
预估：<b>{record.OriginalAmount.USDT_To_TRX(rate, BotHandler.UpdateHandlers.FeeRate)} {record.ConvertCurrency}</b>
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
