using ZHWB.Infrastructure;
namespace ZHWB.Domain.Models
{
    
    public class UserRole:Model
    {
        [ForeignKeyAttribute("User")]
        public string userid{get;set;}
        [ForeignKeyAttribute("Role")]
        public string roleid{get;set;}
    }
}