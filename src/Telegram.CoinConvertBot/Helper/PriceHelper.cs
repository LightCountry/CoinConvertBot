using Telegram.CoinConvertBot.Extensions;

namespace Telegram.CoinConvertBot.Helper
{
    public static class PriceHelper
    {
        public static decimal USDT_To_TRX(this decimal from, decimal rate, decimal feeRate = 0.1m)
        {
            from = from.ToRoundNegative(6);
            rate = rate.ToRoundNegative(2);
            var fee = Math.Max(from * 0.01m, 1m);
            return ((from - fee) * rate * (1 - feeRate)).ToRoundNegative(2);
        }
        public static decimal TRX_To_USDT(this decimal from, decimal rate, decimal feeRate = 0.1m)
        {
            from = from.ToRoundNegative(6);
            rate = rate.ToRoundNegative(2);
            var usdt = from / rate / (1 - feeRate);
            var fee = Math.Max(usdt * 0.01m, 1m);
            return (usdt + fee).ToRoundNegative(2);
        }
    }
}
