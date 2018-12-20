using Microsoft.Extensions.Caching.Distributed;
using ZHWB.Infrastructure.MessageQueue;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using ZHWB.Infrastructure.Log;
namespace ZHWB.Infrastructure.Cache
{
    /// <summary>
    /// 缓存，涉及到Mysql每个表的上层操作优先于Mysql更新和查询
    /// Redis数据操作
    /// 数据存储规则：
    /// 常规缓存String:Key-Value(JSON)
    /// 单条数据HASH：KEY(表名:键值)-VALUE(HASH)
    /// 一对多关系映射Set:KEY(主表1名称_表2名称:表1键值)-VALUE(表2键值[]) 根据业务(仅本项目)一个Set理论不会超过10000
    /// </summary>
    /// <typeparam name="T">存储模型</typeparam>
    public class DataCaches<T> : IDataCaches<T> where T : Model, new()
    {
        private string[] props;
        private Dictionary<string, string> relationfields = new Dictionary<string, string>();
        private string Tbl_Name = typeof(T).Name;

        public DataCaches()
        {
            var propinfos = typeof(T).GetProperties();
            props = propinfos.Select(s => s.Name).ToArray();
            var cols = propinfos.Where(s => s.GetCustomAttribute(typeof(ForeignKeyAttribute)) != null).ToList();
            foreach (var col in cols)
            {
                var attrs = cols.Select(s => s.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute).FirstOrDefault();
                if (attrs != null && attrs.Table != null)
                {
                    relationfields.Add(col.Name, attrs.Table);
                }
            }
        }

        /// <summary>
        /// 设置普通缓存默认1天过期
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiredays">过期天数</param>
        public void SetValue(string key, string value, int expiredays = 1)
        {
            var newkey = "Cache:" + Tbl_Name + ":" + key;//加前缀防止KEY重复
            RedisHelper.Set(newkey, value);
            RedisHelper.Expire(newkey, TimeSpan.FromDays(expiredays));//查询缓存
        }
        public void DelKey(string key)
        {
            var newkey = "Cache:" + Tbl_Name + ":" + key;//加前缀防止KEY重复
            RedisHelper.Del(newkey);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            var newkey = "Cache:" + Tbl_Name + ":" + key;
            return RedisHelper.Get(newkey);
        }
        /// <summary>
        /// 从缓存中读取数据库记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetData(string id)
        {
            var key = Tbl_Name + ":" + id;
            var res = RedisHelper.HMGet(key, props);
            if (res != null)
            {
                return ConvertHash2Model(res);
            }
            return null;
        }

        /// <summary>
        /// 批量获取默认1000
        /// </summary>
        /// <returns></returns>
        public List<T> GetList(int count = 1000)
        {
            if (count > 50000)
            {
                return null;
            }
            var scan = RedisHelper.Scan(0, Tbl_Name + ":*", count);
            var keys = scan.Items;
            var pipe = RedisHelper.StartPipe();
            foreach (var key in keys)
            {
                pipe.HMGet(key, props);
            }
            var res = new List<T>();
            foreach (var l in pipe.EndPipe())
            {
                var r = ConvertHash2Model(l as string[]);
                res.Add(r);
            }
            return res;
        }
        public string[] GetList(string id, string tableName)
        {
            var key = Tbl_Name + "_" + tableName + ":" + id;
            var res = RedisHelper.SScan(key, 0);
            return res.Items;
        }
        public List<T> GetList(string[] ids)
        {
            var res = new List<T>();
            if (ids != null)
            {
                var pipe = RedisHelper.StartPipe();
                for (var i = 0; i < ids.Length; i++)
                {
                    var key = Tbl_Name + ":" + ids[i];
                    var data = pipe.HMGet(key, props);
                }
                foreach (var l in pipe.EndPipe())
                {
                    var r = ConvertHash2Model(l as string[]);
                    res.Add(r);
                }
            }
            return res;
        }
        /// <summary>
        /// 批量设置
        /// </summary>
        /// <param name="list"></param>
        /// <param name="expiredays"></param>
        public void SetList(List<T> list, int expiredays = 2)
        {
            var pipe = RedisHelper.StartPipe();
            foreach (var data in list)
            {
                var key = Tbl_Name + ":" + data.Id;
                object[] val = ConvertModel2Hash(data);
                pipe.HMSet(key, val)
                .Expire(key, TimeSpan.FromDays(expiredays));
                foreach (var dic in relationfields)
                {
                    var relathonkey = Tbl_Name + "_" + dic.Value + ":" + data.Id;
                    object newvalue = data.GetType().GetProperty(dic.Key).GetValue(data);
                    var persist = pipe.HGet(key, dic.Key);
                    pipe.SRem(relathonkey, persist)
                    .SAdd(relathonkey, newvalue).Expire(relathonkey, TimeSpan.FromDays(expiredays));
                }
            }
            pipe.EndPipe();
        }
        /// <summary>
        /// 将数据库记录设置到缓存中
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expiredays"></param>
        public void SetData(T data, int expiredays = 2)//数据缓存
        {
            var key = Tbl_Name + ":" + data.Id;
            var pipe = RedisHelper.StartPipe();
            //更新关系表
            foreach (var dic in relationfields)
            {
                var relathonkey = Tbl_Name + "_" + dic.Value + ":" + data.Id;
                object newvalue = data.GetType().GetProperty(dic.Key).GetValue(data);
                var persist = pipe.HGet(key, dic.Key);
                pipe.SRem(relathonkey, persist)
                .SAdd(relathonkey, newvalue).Expire(relathonkey, TimeSpan.FromDays(expiredays));
            }
            //更新数据
            object[] val = ConvertModel2Hash(data);
            pipe.HMSet(key, val)
            .Expire(key, TimeSpan.FromDays(expiredays)).EndPipe();
        }
        /// <summary>
        /// 移除数据库记录
        /// </summary>
        /// <param name="id"></param>
        public void RemData(string id)
        {
            var data = GetData(id);
            if (data != null)
            {
                var key = Tbl_Name + ":" + data.Id;
                RedisHelper.HDel(key, props);//删除数据
                //更新关系表
                foreach (var dic in relationfields)
                {
                    var relathonkey = Tbl_Name + "_" + dic.Value + ":" + data.Id;
                    object value = data.GetType().GetProperty(dic.Key).GetValue(data);
                    RedisHelper.SRem(relathonkey, value);
                }
            }
        }
        private object[] ConvertModel2Hash(T data)
        {
            if (data != null)
            {
                var res = new List<object>();
                foreach (var name in props)
                {
                    res.Add(name);
                    object val = data.GetType().GetProperty(name).GetValue(data);
                    res.Add(val);
                }
                return res.ToArray();
            }
            return null;
        }
        private T ConvertHash2Model(string[] values)
        {
            if (values != null && props.Length == values.Length)
            {
                var res = new T();
                string name = string.Empty;
                for (var i = 0; i < props.Length; i++)
                {
                    name = props[i];
                    res.GetType().GetProperty(name).SetValue(res, values[i]);
                }
                return res;
            }
            return null;
        }
    }
}