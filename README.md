# .NETCORE.HighConcurrentArchitecture

使用第三方软件

https://dev.mysql.com/downloads/
https://www.rabbitmq.com/
https://redis.io/

https://github.com/2881099/csredis
https://github.com/StackExchange/Dapper

https://github.com/travist/jsencrypt
基于.NET CORE2.1

主要包括JWTTOKEN的验证实现

RSA加密传输

权限中间件

RabbitMQ队列和Mysql数据库连接池

连接池不会每次请求都创建数据库或MQ连接，最多只会创建指定数量的连接，当连接不可用或丢失，将会重新创建连接，并在调用以后重置为可用状态，进入空闲队列以供其它请求使用,
并不会释放。

采用Dapper访问Mysql
采用Redis作为读取缓存
采用RabbitMQ和独立后台进行MySQL写入

使用连接池时一定要注意每次使用一个连接以后(包括MySQL,MQ)，在catch或finally中将连接重置为可用状态
防止查询出现异常时，连接池又恰好满了造成的队列等待死锁。

使用连接池时一定要注意每次使用一个连接以后(包括MySQL,MQ)，在catch或finally中将连接重置为可用状态
防止查询出现异常时，连接池又恰好满了造成的队列等待死锁。

使用连接池时一定要注意每次使用一个连接以后(包括MySQL,MQ)，在catch或finally中将连接重置为可用状态
防止查询出现异常时，连接池又恰好满了造成的队列等待死锁。


Redis缓存实现 RedisCore内部自带连接池

数据同步服务
运行

cd dir

dotnet restore  //获取依赖

dotnet build    //编译  

dotnet run      //运行
 
客户端验证方法：

login:var encrypt = new JSEncrypt();

encrypt.setPublicKey(publickey);

var login={username:"test",password:"test"};

var postdata = encrypt.encrypt(JSON.stringify(login).trim());

ajax方式:headers:{"Authorization": "Bearer "+token_data}

api或signalR方式:requestURL?access_token=token_data
