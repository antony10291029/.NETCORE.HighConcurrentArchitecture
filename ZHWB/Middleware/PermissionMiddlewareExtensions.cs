using Microsoft.AspNetCore.Builder;
namespace ZHWB.Middleware
{
    public static class PermissionMiddlewareExtensions
    {
        /// <summary>
        /// 引入权限中间件
        /// </summary>
        public static IApplicationBuilder UsePermission(
             this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionMiddleware>();
        }
    }
}