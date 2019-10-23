using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ZHWB.Infrastructure.Cache;
using ZHWB.Domain.Models;
using Newtonsoft.Json;
using ZHWB.Domain.Repositories;
using ZHWB.ViewModels;

namespace ZHWB.Middleware
{
    /// <summary>
    /// 授权验证中间件
    /// </summary>
    public class PermissionMiddleware
    {
        /// <summary>
        /// 管道代理对象
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// 权限中间件构造
        /// </summary>
        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private IUserRepository _userRep;
        /// <summary>
        /// 调用管道
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context, IUserRepository userRep)
        {
            _userRep = userRep;
            //请求Url
            var questUrl = context.Request.Path.Value.ToLower();
            //是否经过验证
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            ///已经通过登录验证的API
            if (isAuthenticated)
            {
                string json = context.User.Claims.Where(s => s.Type == ClaimTypes.UserData).First().Value;
                var user = JsonConvert.DeserializeObject<UserInfoViewModel>(json);
                if (user.Roles.Select(s => s.privates).Contains("*"))//验证是否有权限
                {
                    return this._next(context);
                }
                else{
                   context.Response.StatusCode=401;
                }
            }
            return this._next(context);
        }
        private Boolean CheckPermissions(List<Role> roles, string url)
        {
            //这里验证权限
            return true;
        }
    }
}