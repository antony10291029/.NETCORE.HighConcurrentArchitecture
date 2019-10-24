using ZHWB.Infrastructure.MySQL;
using ZHWB.Infrastructure.Cache;
using ZHWB.Domain.Models;
using System.Linq;
using System.Collections.Generic;
using System;
namespace ZHWB.Domain.Repositories
{
    public class UserRoleRepository:IUserRoleRepository
    {
        IDataCaches<UserRole> _cachce;
        IDataRepository _data;
        public UserRoleRepository(IDataCaches<UserRole> cachce,IDataRepository data)
        {
            _cachce=cachce;
            _data=data;
        }
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<UserRole> GetListByUserId(string userid)
        {
            var ids = _cachce.GetList(userid,"User");
            if (ids != null&&ids.Length>0)
            {
                var userroles=_cachce.GetList(ids);
                if(userroles!=null&&userroles.Count>0){
                    return userroles;
                }
            }
            var persist = _data.Query<UserRole>("SELECT * FROM zhwb.userrole  where userid = @id", new { id = userid });
            _cachce.SetList(persist);//同步缓存
            return persist;
        }
        /// <summary>
        /// 设置用户角色
        /// </summary>
        /// <param name="roleids"></param>
        public void SetRoles(string[] roleids, string userid)
        {
            //清空
            var persists=GetListByUserId(userid);
            var ids = persists.Select(s => s.roleid).ToList();
            _data.RemoveList<UserRole>(ids);
            //增加
            if (roleids != null&&roleids.Length>0)
            {
                var list=new List<UserRole>();
                foreach(var id in roleids)
                {
                    var item=new UserRole(){Id=Guid.NewGuid().ToString(),userid=userid,roleid=id};
                    list.Add(item);
                }
                _data.AddList<UserRole>(list);//更新MYSQL
                _cachce.SetList(list);//同步缓存
            }
        }
         
        /// <summary>
        /// 获取具有该角色的用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public List<UserRole> GetUsersByRoleid(string roleid)
        {
            var ids = _cachce.GetList(roleid,"Role");
            if(ids!=null&&ids.Length>0)
            {
                var datas=_cachce.GetList(ids);
                if(datas!=null&&datas.Count>0)
                {
                    return datas;
                }
            }
            var persist = _data.Query<UserRole>("SELECT * FROM zhwb.userrole  where roleid = @id", new { id = roleid });
            _cachce.SetList(persist);
            return persist;
        }
    }
}