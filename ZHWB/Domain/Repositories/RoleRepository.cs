using ZHWB.Infrastructure.Cache;
using ZHWB.Infrastructure.MessageQueue;
using ZHWB.Infrastructure.MySQL;
using ZHWB.Domain.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using ZHWB.Infrastructure;
namespace ZHWB.Domain.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private IDataCaches<Role> _cachce;
        private string TableName = typeof(Role).Name;
        private IDataRepository _data;
        public RoleRepository(IDataCaches<Role> dataCaches, IDataRepository dataRepository)
        {
            _cachce = dataCaches;
            _data = dataRepository;
        }
        public Role GetRole(string id)
        {
            var res = _cachce.GetData(id);
            if (res == null)
            {
                res = _data.Query<Role>("SELECT * FROM zhwb.role  where id = @id limit 1", id).FirstOrDefault();
                if (res != null)
                {
                    _cachce.SetData(res);
                }
            }
            return res;
        }
        public void SaveRole(Role role)
        {
            if (string.IsNullOrEmpty(role.Id))
            {
                role.Id = Guid.NewGuid().ToString();
            }
            _data.SetData(role);
            _cachce.SetData(role);
        }
        public void RemRole(string id)
        {
            _data.RemoveData<Role>(id);
            _cachce.RemData(id);
        }
        public List<Role> GetRoles(string[] ids)
        {
            var datas=_cachce.GetList(ids);
            if(datas!=null){
                return datas;
            }
            else{
                return GetAll().Where(s=>ids.Contains(s.Id)).ToList();
            }
        }
        /// <summary>
        /// 获取全部角色
        /// </summary>
        /// <returns></returns>
        public List<Role> GetAll()
        {
            var res = _cachce.GetList();
            if (res == null)
            {
                var roles= _data.Query<Role>("SELECT * FROM zhwb.role").ToList();
                _cachce.SetList(roles);
                return roles;
            }
            return null;
        }
    }
}