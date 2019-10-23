using System.Net;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Microsoft.AspNetCore.Cors;
using ZHWB.Infrastructure;
using ZHWB.Infrastructure.MessageQueue;
using ZHWB.Domain.Models;
using ZHWB.Domain.Repositories;
using ZHWB.Infrastructure.Cache;
using System.Security.Cryptography;
using Newtonsoft.Json;
using ZHWB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
namespace ZHWB.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAll")]
    
    public class AuthController : ControllerBase
    {
        IUserRepository _repository;
        IUserRoleRepository _userroles;
        IConfiguration Configuration;
        IRoleRepository _roles;
        public AuthController(IUserRepository repository,
         IConfiguration configuration, IUserRoleRepository userroles, IRoleRepository roles)
        {
            Configuration = configuration;
            _repository = repository;
            _userroles = userroles;
            _roles = roles;
        }
        [HttpGet]
        [Authorize]
        public string GetAuth()
        {
            return "ok";
        }
        [HttpGet]
        public string GetKey()
        {
            return Configuration["RSA:publicKey"];
        }

        [HttpPost]
        public ActionResult<string> Register([FromForm]User user)
        {
            var val = user.Validate();
            if (val == null)
            {
                _repository.SaveUser(user);
                return "ok";
            }
            else return val.error;
        }
        [HttpPost]
        [Authorize]
        public void removeUser([FromForm]string id)
        {
             _repository.RemoveUser(id);
        }
        [HttpPost]
        [Authorize("test")]
        public string testPolicy()
        {
            return "ok";
        }
        /// <summary>
        /// 用户token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> Login([FromForm]string logininfo)
        {
            ///公钥私钥对解密用户名密码
            RSATool rSATool = new RSATool(RSAType.RSA, Encoding.UTF8, Configuration["RSA:privateKey"], Configuration["RSA:publicKey"]);
            var decrypt = rSATool.Decrypt(logininfo);
            var login = JsonConvert.DeserializeObject<User>(decrypt);
            var user = _repository.ValidateLogin(login.username, login.password);
            if (user != null)
            {
                var expiredays = int.Parse(Configuration["JWTAuth:expires"]);
                var expire = DateTime.Now.AddDays(expiredays);
                var info = new UserInfoViewModel();
                var roleids = _userroles.GetListByUserId(user.Id).Select(s => s.roleid).ToArray();
                var roles = _roles.GetRoles(roleids);
                info.Roles = roles;
                info.name = user.name;
                info.phone = user.phone;
                info.userid = user.Id;
                info.username = user.name;
                var claims = new[]{
                    new Claim(ClaimTypes.Name,info.username),//用户名不重复
                    new Claim(ClaimTypes.UserData,JsonConvert.SerializeObject(info)) 
                };
                //sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
                var secretKey = Configuration["JWTAuth:secretKey"];
                var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256Signature);
                //.NET Core’s JwtSecurityToken class takes on the heavy lifting and actually creates the token.
                var token = new JwtSecurityToken(
                    issuer: Configuration["JWTAuth:issuer"],
                    audience: Configuration["JWTAuth:audience"],
                    claims: claims,
                    expires: expire.ToUniversalTime(),
                    signingCredentials: creds
                );
                var res = new JwtSecurityTokenHandler().WriteToken(token);
                return res;
            }
            return null;
        }
    }
}