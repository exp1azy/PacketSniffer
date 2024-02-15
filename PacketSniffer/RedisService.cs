using StackExchange.Redis;

namespace PacketSniffer
{
    /// <summary>
    /// Сервис, представляющий логику для взаимодействия с сервером Redis.
    /// </summary>
    public class RedisService
    {
        private IDatabase _db;
        private ConnectionMultiplexer? _connection;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Логи.</param>
        public RedisService(ConnectionMultiplexer connection)
        {
            _connection = connection;
            _db = _connection.GetDatabase();
        }

        /// <summary>
        /// Добавляет массив <see cref="NameValueEntry"/> в поток Redis по ключу <see cref="RedisKey"/>.
        /// </summary>
        /// <param name="key">Ключ потока.</param>
        /// <param name="streamPairs">Данные.</param>
        /// <returns></returns>
        public async Task StreamAddAsync(RedisKey key, NameValueEntry[] streamPairs) =>
            await _db.StreamAddAsync(key, streamPairs);
    }
}
