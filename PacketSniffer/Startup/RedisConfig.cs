using PacketSniffer.Resources;
using StackExchange.Redis;

namespace PacketSniffer.Startup
{
    public static class RedisConfig
    {
        public static void AddRedis(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton(sp =>
            {
                var connectionString = builder.Configuration["RedisConnection"];
                if (string.IsNullOrEmpty(connectionString))               
                    throw new ArgumentNullException(Error.FailedToReadRedisConnectionString);
                
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }
    }
}
