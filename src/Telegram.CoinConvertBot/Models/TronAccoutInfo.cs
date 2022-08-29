using TronNet.Protocol;

namespace Telegram.CoinConvertBot.Models
{
    public class TronAccoutInfo
    {
        public string Address { get; set; } = null!;
        public decimal TRX { get; set; }
        public decimal USDT { get; set; }
        public AccountResourceMessage Resource { get; set; } = null!;
    }
}
