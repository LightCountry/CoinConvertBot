using Flurl;
using Flurl.Http;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.CoinConvertBot.BgServices.Base;
using Telegram.CoinConvertBot.Domains.Tables;
using Telegram.CoinConvertBot.Helper;

namespace Telegram.CoinConvertBot.BgServices
{
    public class UpdateRateService : BaseScheduledService
    {
        const string baseUrl = "https://www.binance.com";
        const string User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36";
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UpdateRateService> _logger;
        private readonly FlurlClient client;
        public UpdateRateService(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<UpdateRateService> logger) : base("更新汇率", TimeSpan.FromMinutes(10), logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            var WebProxy = configuration.GetValue<string>("WebProxy");
            client = new FlurlClient();
            client.Settings.Timeout = TimeSpan.FromSeconds(15);
            if (!string.IsNullOrEmpty(WebProxy))
            {
                client.Settings.HttpClientFactory = new ProxyHttpClientFactory(WebProxy);
            }

        }

        protected override async Task ExecuteAsync()
        {
            var list = new List<TokenRate>();
            _logger.LogInformation("------------------{tips}------------------", "开始更新汇率");
            using IServiceScope scope = _serviceProvider.CreateScope();
            var _repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TokenRate>>();

            var rate = _configuration.GetValue("TrxRate", 0m);
            if (rate > 0)
            {
                list.Add(new TokenRate
                {
                    Id = $"USDT_{Currency.TRX}",
                    Currency = Currency.USDT,
                    ConvertCurrency = Currency.TRX,
                    LastUpdateTime = DateTime.Now,
                    Rate = rate,
                    ReverseRate = 1m / rate,
                });
            }
            else
            {
                try
                {
                    var convert1 = await baseUrl
                        .WithClient(client)
                        .WithHeaders(new { User_Agent })
                        .AppendPathSegment("bapi/margin/v2/public/new-otc/get-quote")
                        //.SetQueryParams()
                        .PostJsonAsync(new
                        {
                            fromCoin = Currency.USDT.ToString(),
                            toCoin = Currency.TRX.ToString(),
                            requestAmount = 100,
                            requestCoin = Currency.USDT.ToString(),
                        })
                        .ReceiveJson<Root>();
                    if (convert1.success)
                    {
                        list.Add(new TokenRate
                        {
                            Id = $"USDT_{Currency.TRX}",
                            Currency = Currency.USDT,
                            ConvertCurrency = Currency.TRX,
                            LastUpdateTime = DateTime.Now,
                            Rate = convert1.data.quotePrice,
                            ReverseRate = convert1.data.inversePrice,
                        });
                    }
                    else
                    {
                        _logger.LogWarning("TRX -> USDT 汇率获取失败！错误信息：{msg}\n详细错误信息：{msg2}", convert1.message ?? "无", convert1.messageDetail ?? "无");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning("TRX -> USDT 汇率获取失败！错误信息：{msg}", e?.InnerException?.Message + "; " + e?.Message);
                }

                foreach (var item in list)
                {
                    _logger.LogInformation("更新汇率，{a} -> {b} = {c}", item.Currency, item.ConvertCurrency, item.Rate);
                    await _repository.InsertOrUpdateAsync(item);
                }
                _logger.LogInformation("------------------{tips}------------------", "结束更新汇率");
            }
        }
#pragma warning disable CS8618
        public class Root
        {
            public string code { get; set; }
            public string message { get; set; }
            public string messageDetail { get; set; }
            public Data data { get; set; }
            public bool success { get; set; }
        }
        public class Data
        {
            public decimal quotePrice { get; set; }
            public decimal inversePrice { get; set; }
            public int expireTime { get; set; }
            public long expireTimestamp { get; set; }
            public string fromCoin { get; set; }
            public string toCoin { get; set; }
            public decimal toCoinAmount { get; set; }
            public decimal fromCoinAmount { get; set; }
            public string requestCoin { get; set; }
            public decimal requestAmount { get; set; }
            public bool fromIsBase { get; set; }
            public bool split { get; set; }
        }
#pragma warning restore CS8618
    }
}
