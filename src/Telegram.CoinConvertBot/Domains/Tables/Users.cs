using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.CoinConvertBot.Domains.Tables
{
    /// <summary>
    /// 用户表
    /// </summary>
    public class Users
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [Column(IsPrimary = true)]
        public long UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// 名
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// 姓
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false, Position = -100)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(ServerTime = DateTimeKind.Utc, Position = -100)]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(Position = -110)]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        [Column(Position = -110)]
        public DateTime? DeletedTime { get; set; }
    }
}
