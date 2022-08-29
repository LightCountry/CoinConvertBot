namespace Telegram.CoinConvertBot.Domains
{
    public interface IEntity : ISoftDelete
    {
        /// <summary>
        /// ID
        /// </summary>
        Guid Id { get; set; }
    }
}
