using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.CoinConvertBot.Domains
{
    /// <summary>
    /// 实体类基本类型
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Position = 1)]
        public Guid Id { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false, Position = -100)]
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 软删除
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
