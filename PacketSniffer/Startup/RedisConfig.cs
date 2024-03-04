using PacketSniffer.Resources;
using StackExchange.Redis;

namespace PacketSniffer.Startup
{
    public static class RedisConfig
    {
        public static ConnectionMultiplexer? AddRedis(this WebApplicationBuilder builder)
        {
            ConnectionMultiplexer? connection = null;

            builder.Services.AddSingleton(sp =>
            {
                var connectionString = builder.Configuration["RedisConnection"];
                if (string.IsNullOrEmpty(connectionString))               
                    throw new ArgumentNullException(Error.FailedToReadRedisConnectionString);

                connection = ConnectionMultiplexer.Connect(connectionString);
                return connection;
            });

            return connection;
        }
    }
}
