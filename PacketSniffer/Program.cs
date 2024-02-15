using PacketSniffer.Startup;
using Serilog;

namespace PacketSniffer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.RunAsProcess();
            builder.StartWithWindows();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventLog("PacketSniffer", manageEventSource: true)
                .CreateLogger();

            builder.AddRedis();

            builder.Services.AddSingleton<PcapAgent>();
            builder.Services.AddTransient<RedisService>();

            builder.Services.AddAuthentication();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseRouting();

            app.MapControllers();

            app.Run();
        }
    }
}
