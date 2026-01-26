using Talentree.API.Middlewares;

namespace Talentree.API.Extentions
{
    /// <summary>
    /// Extension method to register the middleware
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
