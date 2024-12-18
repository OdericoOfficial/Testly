using Serilog;

namespace Testly.Smtp.Api
{
    internal static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddModule(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            builder.Host.UseOrleans(siloBuilder =>
            {
                siloBuilder.UseAdoNetClustering();
                siloBuilder.UseAdoNetReminderService();
                siloBuilder.AddAdoNetGrainStorageAsDefault();
                siloBuilder.AddAdoNetStreams(nameof(Stream));
            });
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            return builder;
        }
    }
}
