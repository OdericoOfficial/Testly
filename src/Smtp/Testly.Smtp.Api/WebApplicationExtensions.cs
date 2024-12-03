namespace Testly.Smtp.Api
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseApiModule(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Map("/dashborad", builder => builder.UseOrleansDashboard());
            return app; 
        }
    }
}
