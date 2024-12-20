using System.Net;
using Orleans.Configuration;
using Orleans.Runtime.Hosting;
using Serilog;
using StackExchange.Redis;

namespace Testly.Smtp.Api
{
    internal static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddModule(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            builder.AddOrleans();
            builder.Services.AddApplicaiton();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            return builder;
        }

        internal static WebApplicationBuilder AddOrleans(this WebApplicationBuilder builder)
        {
            builder.Host.UseOrleans(siloBuilder =>
            {
                var redis = builder.Configuration.GetConnectionString("Redis")!;
                
                siloBuilder.AddRedisGrainStorageAsDefault(optionsBuilder =>
                {
                    optionsBuilder.ConfigurationOptions = ConfigurationOptions.Parse(redis);
                    optionsBuilder.ConfigurationOptions.DefaultDatabase = 0;
                });
                siloBuilder.AddMemoryGrainStorage("PubSubStore");
                siloBuilder.AddMemoryStreams("PubSubStore");
                siloBuilder.UseRedisReminderService(optionsBuilder =>
                {
                    optionsBuilder.ConfigurationOptions = ConfigurationOptions.Parse(redis);
                    optionsBuilder.ConfigurationOptions.DefaultDatabase = 1;
                });
                siloBuilder.UseRedisClustering(optionsBuilder =>
                {
                    optionsBuilder.ConfigurationOptions = ConfigurationOptions.Parse(redis);
                    optionsBuilder.ConfigurationOptions.DefaultDatabase = 2;
                });
            });
            return builder;
        }
    }
}
