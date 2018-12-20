using System;
using System.Collections.Generic;
using System.Linq;
namespace ZHWB.Infrastructure
{
    /// <summary>
    /// 此属性表示需要增加映射以方便Redis查询表关系
    /// </summary>
    public class ForeignKeyAttribute:Attribute
    {
        private string table;
        public ForeignKeyAttribute(string Table)
        {
            table=Table;
        }
        public string Table{get{return table;}}
    }
}