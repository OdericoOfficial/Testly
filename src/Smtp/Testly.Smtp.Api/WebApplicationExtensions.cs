namespace Testly.Smtp.Api
{
    internal static class WebApplicationExtensions
    {
        public static WebApplication UseModule(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseAuthorization();
            app.MapControllers();
            return app;
        }
    }
}
