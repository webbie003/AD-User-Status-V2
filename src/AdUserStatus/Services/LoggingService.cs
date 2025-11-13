using Microsoft.Extensions.Logging;

namespace AdUserStatus.Services
{
    public static class LoggingService
    {
        public static ILogger CreateLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            return loggerFactory.CreateLogger("AdUserStatus");
        }
    }
}
