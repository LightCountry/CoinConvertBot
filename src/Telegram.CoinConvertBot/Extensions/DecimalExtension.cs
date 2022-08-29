namespace Telegram.CoinConvertBot.Extensions
{
    public static class DecimalExtension
    {
        public static decimal ToRoundNegative(this decimal value, int decimals = 4)
        {
            return Math.Round(value, decimals, MidpointRounding.ToNegativeInfinity);
        }
    }
}
