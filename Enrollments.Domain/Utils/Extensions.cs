using Microsoft.AspNetCore.Builder;

namespace Enrollments.Domain.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Setups a middleware to your web application pipeline to replace "ADDHOST/" on links tagged with LinkJsonConverter.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your Configure method</param>
        public static IApplicationBuilder SetupLinkJsonConverter(this IApplicationBuilder app)
        {
            app.UseWhen(context => LinkJsonConverter.SchemeHostIsNullOrEmpty, app =>
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                        LinkJsonConverter.SchemeHost = $"{context.Request.Headers["X-Forwarded-Proto"][0]}://{context.Request.Host.Value}/v2";
                    await next.Invoke();
                });
            });
            return app;
        }
    }
}