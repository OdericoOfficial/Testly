using Serilog;

namespace Testly.Smtp.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(configure => configure.Console())
                .WriteTo.Async(configure => configure.Seq("http://seq"))
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.AddApiModule();
                var app = builder.Build();
                app.UseApiModule();
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                if (ex is HostAbortedException)
                    throw;
                Log.Error(ex, "{ApiModule} catch unexpected exception.", nameof(Smtp));
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
