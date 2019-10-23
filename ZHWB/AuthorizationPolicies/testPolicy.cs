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
                                return true;
                            }
                            return false;
                        });
            });
        }
    }

}