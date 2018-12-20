using ZHWB.Infrastructure.Cache;
using ZHWB.Domain.Models;
using System.Collections.Generic;
namespace ZHWB.Domain.Repositories
{
    public interface IUserRepository
    {
        User GetUser(string id);
        void RemoveUser(string id);
        void SaveUser(User user);
        User ValidateLogin(string uname, string pwd);
        void SetUserTokenInfo(string userid, string tokendata);
        string GetUserTokenInfo(string userid);
    }
}