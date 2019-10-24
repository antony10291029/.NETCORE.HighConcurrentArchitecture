# .NETCORE.HighConcurrentArchitecture

一个基础的.NET CORE 轻量级高并发服务端结构
上手简单易用无门槛

实际使用请根据系统业务情况合理修改 日志和单元测试根据喜好自行完善

联系作者:290330505@qq.com

使用第三方软件

https://dev.mysql.com/downloads/
https://www.rabbitmq.com/
https://redis.io/

https://github.com/2881099/csredis
https://github.com/StackExchange/Dapper

https://github.com/travist/jsencrypt

测试工具：http://jmeter.apache.org/download_jmeter.cgi

20191023

--更新至.NET CORE3.0 

新增自定义授权策略实现权限控制弃用自定义的权限中间件

新增数据库创建脚本 Redis和MQ连接池的实现

修复3.0版本下的身份认证和策略授权变化

mySQL数据库存储规则:

    表名称和字段与模型类一一对应(名称一致表名类名一致)
    
redis缓存数据存储规则：

    常规缓存String:Key-Value(JSON)

    单条数据HASH：KEY(表名:键值)-VALUE(HASH)

    一对多关系映射Set:KEY(主表1名称_表2名称:表1键值)-VALUE(表2键值[]) 
    
    主表字段标记属性[ForeignKeyAttribute("从表名称")]

数据同步服务

运行

cd dir

dotnet restore  //获取依赖

dotnet build    //编译  

dotnet run      //运行

![image](https://github.com/luoyuzhao/.NETCORE.HighConcurrentArchitecture/blob/master/Screenshot.jpg?raw=true)
 



