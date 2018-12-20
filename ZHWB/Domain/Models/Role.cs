using ZHWB.Infrastructure;
namespace ZHWB.Domain.Models
{
    public class Role:Model
    {
        public string name{get;set;}
        public string des{get;set;}
        public string privates{get;set;}//多个用#分割
    }
}