namespace ZHWB.Domain
{
    /// <summary>
    /// 权限
    /// </summary>
    public static class Permissions
    {
        public static string Admin{//超级管理员
            get {return "*";}
        }
        public static string EditUser{//编辑用户
            get {return "EditUser";}
        }
        public static string SetUserRole{//设置角色
            get {return "SetUserRole";}
        }
        public static string EditRole{//编辑角色
             get {return "EditRole";}
        }
        public static string FileServerAccess{//文件服务器权限
            get {return "FileServerAccess";}
        }
        public static string EditRelic{//编辑文物信息
            get {return "EditRelic";}
        }
        public static string EditRelicType{//编辑文物分类
            get {return "EditRelicType";}
        }
         public static string EditMuseumPosition{//编辑全景图地址
            get {return "EditMuseumPosition";}
        }
    }
}