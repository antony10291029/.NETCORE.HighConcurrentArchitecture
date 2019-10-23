using System.Collections.Generic;
using ZHWB.Domain.Models;
using System;
namespace ZHWB.ViewModels
{
    /// <summary>
    /// 登录后的用户信息包括权限不包括密码(RSA加密传输)
    /// </summary>
    public class UserInfoViewModel
    {
        public List<Role> Roles { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
    }
}