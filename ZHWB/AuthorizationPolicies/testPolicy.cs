using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using ZHWB.Domain.Repositories;
using System;
using Newtonsoft.Json;
using ZHWB.ViewModels;
namespace ZHWB.AuthorizationPolicies
{

    /// <summary>
    /// 测试自定义授权策略
    /// </summary>
    public static class testPolicy
    {
        public static void AddtestPolicy(this AuthorizationOptions options, IUserRepository userRep)
        {
            options.AddPolicy("test", (builder) =>
            {
                builder.RequireAuthenticatedUser().RequireAssertion(context =>
                        {
                            if (context.User.Identity.IsAuthenticated)
                            {
                                string json = context.User.Claims.Where(s => s.Type == ClaimTypes.UserData).First().Value;
                                var user = JsonConvert.DeserializeObject<UserInfoViewModel>(json);
                                if (user.Roles.Select(s => s.privates).Contains("*"))//验证是否有权限
                                    return true;
                            }
                            return false;
                        });
            });
        }
    }

}