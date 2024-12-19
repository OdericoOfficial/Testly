using Serilog;
using System.Runtime.CompilerServices;

namespace Testly.Smtp.Api
{
    internal static class WebApplicationBuilderExtensions
    {
        internal static class Constants
        {
            public const string Store = nameof(Store);
            public const string StateStore = nameof(StateStore);
            public const string PubSubStore = nameof(PubSubStore);
            public const string ClusteringStore = nameof(ClusteringStore);
            public const string ReminderStore = nameof(ReminderStore);
            public const string Invariant = "Microsoft.Data.SqlClient";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string GetConnectStringWithDatabase(string connectString, string store)
                => $"{connectString}Database={store};";
        }

        public static WebApplicationBuilder AddModule(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            builder.Host.UseOrleans(siloBuilder =>
            {
                var connectString = builder.Configuration.GetConnectionString(Constants.Store)!;

                siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.ConnectionString = Constants.GetConnectStringWithDatabase(connectString, Constants.StateStore);
                    options.Invariant = Constants.Invariant;
                });

                siloBuilder.AddAdoNetGrainStorage(Constants.PubSubStore, options =>
                {
                    options.ConnectionString = Constants.GetConnectStringWithDatabase(connectString, Constants.PubSubStore);
                    options.Invariant = Constants.Invariant;
                });

                siloBuilder.AddAdoNetStreams(Constants.PubSubStore, options =>
                {
                    options.ConnectionString = Constants.GetConnectStringWithDatabase(connectString, Constants.PubSubStore);
                    options.Invariant = Constants.Invariant;
                });

                siloBuilder.UseAdoNetClustering(options =>
                {
                    options.ConnectionString = Constants.GetConnectStringWithDatabase(connectString, Constants.ClusteringStore);
                    options.Invariant = Constants.Invariant;
                });

                siloBuilder.UseAdoNetReminderService(options =>
                {
                    options.ConnectionString = Constants.GetConnectStringWithDatabase(connectString, Constants.ReminderStore);
                    options.Invariant = Constants.Invariant;
                });
            });
            builder.Services.AddApplicaiton();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            return builder;
        }
    }
}
