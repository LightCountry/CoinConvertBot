using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Telegram.CoinConvertBot.Domains.Tables
{
    public class TokenRate
    {
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// 原币种
        /// </summary>
        [Column(MapType = typeof(string))]
        public Currency Currency { get; set; }
        /// <summary>
        /// 转换币种
        /// </summary>
        [Column(MapType = typeof(string))]
        public Currency ConvertCurrency { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        [Column(Precision = 24, Scale = 12)]
        public decimal Rate { get; set; }
        /// <summary>
        /// 反向汇率
        /// </summary>
        [Column(Precision = 24, Scale = 12)]
        public decimal ReverseRate { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }

    public enum FiatCurrency
    {
        CNY = 10,
        USD
    }

    public enum Currency
    {
        BTC = 10,
        ETH,
        TRX,
        USDT,
    }
}
