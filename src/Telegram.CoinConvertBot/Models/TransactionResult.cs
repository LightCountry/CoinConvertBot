using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.CoinConvertBot.Models
{
    public class TransactionResult : TransactionResult<string>
    {
    }
    public class TransactionResult<T>
    {
        public bool Success { get; set; }
#pragma warning disable CS8618 
        public T Data { get; set; }
        public string Message { get; set; }
#pragma warning restore CS8618
    }
}
