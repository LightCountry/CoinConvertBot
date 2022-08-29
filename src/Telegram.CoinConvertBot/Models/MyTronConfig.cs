
namespace Telegram.CoinConvertBot.Models
{
    public class MyTronConfig
    {
        public string PrivateKey { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string USDTContractAddress { get; set; } = null!;
        public string ToAddress { get; set; } = null!;
        public string ApiHost { get; set; } = null!;
    }
}
