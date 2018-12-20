using ZHWB.Domain.Models;
using ZHWB.Infrastructure.Cache;
using ZHWB.Infrastructure.Log;
using ZHWB.Infrastructure.MessageQueue;
using ZHWB.Infrastructure.MySQL;
using System.Collections.Generic;
using System;
using System.Linq;
using ZHWB.Infrastructure;
using Newtonsoft.Json;
using System.Security.Cryptography;
namespace ZHWB.Domain.Repositories
{

    public class UserRepository : IUserRepository
    {
        private IDataCaches<User> _cachce;
        private string TableName = typeof(User).Name;
        private IDataRepository _data;
        public UserRepository(IDataCaches<User> dataCaches, IDataRepository dataRepository)
        {
            _cachce = dataCaches;
            _data = dataRepository;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetUser(string id)
        {
            var res = _cachce.GetData(id);
            if (res == null)
            {
                res = _data.Query<User>("SELECT * FROM zhwb.user  where id = @id limit 1", id).FirstOrDefault();
                if (res != null)
                {
                    _cachce.SetData(res);
                }
            }
            return res;
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void SaveUser(User user)
        {
            _data.SetData(user);
            _cachce.SetData(user);
        }

        public void RemoveUser(string id)
        {
            var user = _cachce.GetData(id);
            if (user != null && user.username == "admin")
            {
                return;//超级用户禁止删除
            }
            else
            {
                _data.RemoveData<User>(id);
                _cachce.RemData(id);//移除缓存
            }
        }
        /// <summary>
        /// 缓存用户TOKEN
        /// </summary>
        public void SetUserTokenInfo(string userid, string tokendata)
        {
            _cachce.SetValue("INFO_USER:" + userid, tokendata);
        }
        public string GetUserTokenInfo(string userid)
        {
            return _cachce.GetValue("INFO_USER:" + userid);
        }
        public User ValidateLogin(string uname, string pwd)
        {
            var key = "Login:" + uname + ":" + pwd;
            var data = _cachce.GetValue(key);
            if (data != null)
            {
                return JsonConvert.DeserializeObject<User>(data);
            }
            else
            {
                var res = _data.Query<User>("SELECT * FROM zhwb.user  where username = @username and password = @password limit 1",
                                new { username = uname, password = pwd }
                            ).FirstOrDefault();
                if (res != null)
                {
                    _cachce.SetValue(key, JsonConvert.SerializeObject(res));
                }
                return res;
            }
        }
    }
}