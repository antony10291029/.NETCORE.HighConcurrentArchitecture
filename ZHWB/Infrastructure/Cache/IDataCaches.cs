using System.Collections.Generic;
using System.Linq.Expressions;
using System;
namespace ZHWB.Infrastructure.Cache
{
    public interface IDataCaches<T>  where T:Model 
    {
        void SetData(T data,int expiredays=2);
        T GetData(string id);
        void RemData(string id);
        void DelKey(string key);
        string GetValue(string key);
        void SetValue(string key,string value,int expiredays=1);
        List<T> GetList(int count = 1000);
        List<T> GetList(string[] ids);
        string[] GetList(string id,string tableName);
        void SetList(List<T> list, int expiredays = 2);
    }
}