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
                siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("StateStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.AddAdoNetGrainStorage("PubSubStore", options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("PubSubStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.AddAdoNetGrainStorage("ClusteringStore", options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("ClusteringStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.AddAdoNetGrainStorage("ReminderStore", options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("ReminderStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.AddAdoNetStreams("PubSubStore", options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("PubSubStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.UseAdoNetClustering(options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("ClusteringStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });

                siloBuilder.UseAdoNetReminderService(options =>
                {
                    options.ConnectionString = builder.Configuration.GetConnectionString("ReminderStore");
                    options.Invariant = "Microsoft.Data.SqlClient";
                });
            });
            builder.Services.AddApplicaiton();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            return builder;
        }
    }
}
