using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Telegram.CoinConvertBot.Domains.Tables
{
    public class TokenRecord : Entity
    {
        /// <summary>
        /// 区块唯一编号
        /// </summary>
        [Key]
        public string BlockTransactionId { get; set; } = null!;
        /// <summary>
        /// 原始币种
        /// </summary>
        [Column(MapType = typeof(string))]
        public Currency OriginalCurrency { get; set; }
        /// <summary>
        /// 转换币种
        /// </summary>
        [Column(MapType = typeof(string))]
        public Currency ConvertCurrency { get; set; }
        /// <summary>
        /// 原始金额
        /// </summary>
        [Column(Precision = 15, Scale = 6)]
        public decimal OriginalAmount { get; set; }
        /// <summary>
        /// 转换金额
        /// </summary>
        [Column(Precision = 15, Scale = 6)]
        public decimal ConvertAmount { get; set; }
        /// <summary>
        /// 钱包地址
        /// </summary>
        public string ToAddress { get; set; } = null!;
        /// <summary>
        /// 来源地址
        /// </summary>
        public string FromAddress { get; set; } = null!;
        /// <summary>
        /// 入账时间
        /// </summary>
        public DateTime ReceiveTime { get; set; }
        /// <summary>
        /// 入账备注
        /// </summary>
        public string? Memo { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }
        /// <summary>
        /// 支付id
        /// </summary>
        public string? Txid { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
        /// <summary>
        /// 是否通知
        /// </summary>
        public bool Notify { get; set; }
    }
    public enum Status
    {

        Pending,
        Paid,
        Error
    }
}
