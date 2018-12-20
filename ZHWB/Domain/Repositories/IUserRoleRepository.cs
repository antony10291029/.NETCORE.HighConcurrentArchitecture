using System.Collections.Generic;
using ZHWB.Domain.Models;
namespace ZHWB.Domain.Repositories
{
    public interface IUserRoleRepository
    {
        List<UserRole> GetListByUserId(string userid);
        void SetRoles(string[] roleids, string userid);
        List<UserRole> GetUsersByRoleid(string roleid);

    }
}