
using Serilog;

namespace Testly.Smtp.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.AddModule();
                var app = builder.Build();
                app.UseModule();
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                if (ex is HostAbortedException)
                    throw;

                Log.Error(ex, "Unexpected error catch outside.");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
