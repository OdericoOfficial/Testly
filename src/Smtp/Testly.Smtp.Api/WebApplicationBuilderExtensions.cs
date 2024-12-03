using Serilog;
using Testly.Smtp.Application;

namespace Testly.Smtp.Api
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddApiModule(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
            builder.Services.AddApplicationModule();
            builder.Services.AddRegisteredService();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            return builder;
        }
    }
}
