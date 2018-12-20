using ZHWB.Infrastructure;

namespace ZHWB.Domain.Models
{
    public class User:Model
    {
        public string username{get;set;}
        public string password{get;set;}
        public string name{get;set;}
        public string phone{get;set;}
        
    }
}