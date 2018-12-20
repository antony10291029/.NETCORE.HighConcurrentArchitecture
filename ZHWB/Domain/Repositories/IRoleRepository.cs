using ZHWB.Infrastructure.MySQL;
using ZHWB.Domain.Models;
using System.Collections.Generic;
namespace ZHWB.Domain.Repositories
{
    public interface IRoleRepository
    {
        Role GetRole(string id);
        void SaveRole(Role role);
        void RemRole(string id);
        List<Role> GetAll();
        List<Role> GetRoles(string[] ids);
    }
}