# .NETCORE.HighConcurrentArchitecture

Based On .NET CORE 2.1 CLI

include 

1.JWT AUTHROIZE 

2.RSA Encrypt 

3.Permission/token expire Authorize Middleware

3.RabbitMQ ConnectionPool

4.Redis Cache

5.MySQL Database ConnectionPool

6.Sync Service

7.SignalR Core

基于.NET CORE2.1

主要包括JWTTOKEN的验证实现

RSA加密传输

权限中间件

RabbitMQ队列和Mysql数据库连接池

Redis缓存实现

数据同步服务
运行

cd dir

dotnet restore

dotnet build

dotnet run
 
客户端验证方法：

login:var encrypt = new JSEncrypt();

encrypt.setPublicKey(publickey);

var login={username:"test",password:"test"};

var postdata = encrypt.encrypt(JSON.stringify(login).trim());

ajax方式:headers:{"Authorization": "Bearer "+token_data}

api或signalR方式:requestURL?access_token=token_data
