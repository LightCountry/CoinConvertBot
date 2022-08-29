namespace Telegram.CoinConvertBot.Domains.Tables
{
    public class TokenBind : Entity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 昵称
        /// </summary>
        public string FullName { get; set; } = null!;
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; } = null!;
        /// <summary>
        /// 地址类型
        /// </summary>
        public Currency Currency { get; set; }
    }
}
