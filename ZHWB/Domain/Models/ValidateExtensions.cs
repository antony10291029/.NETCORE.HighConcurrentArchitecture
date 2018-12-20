using ZHWB.Infrastructure;
namespace ZHWB.Domain.Models
{
    /// <summary>
    /// 输入验证扩展
    /// </summary>
    public static class ValidateExtensions
    {
        public static ValidateError Validate(this User user){
            if(string.IsNullOrEmpty(user.name))
            {
                return new ValidateError("name","名称为空");
            }
            if(string.IsNullOrEmpty(user.password))
            {
                return new ValidateError("password","密码为空");
            }
            if(string.IsNullOrEmpty(user.username))
            {
                return new ValidateError("username","用户名为空");
            }
            return null;
        }
        public static ValidateError Validate(this Role role){
            if(string.IsNullOrEmpty(role.name))
            {
                return new ValidateError("name","名称为空");
            }
            return null;
        }
    }
}