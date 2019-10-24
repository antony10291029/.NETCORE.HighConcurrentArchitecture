using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using ZHWB.Infrastructure.Cache;
using ZHWB.Domain.Models;
using System.Linq;
namespace ZHWB.MessageHubs
{
    /// <summary>
    /// 即时消息通知服务
    /// 此类不支持方法重载！
    /// </summary>
    public class NotifyHub : Hub
    {
        IDataCaches<User> _cache;
        private readonly string prefix="USER_CONN:";
        public NotifyHub(IDataCaches<User> cache)
        {
            _cache = cache;
        }
        /// <summary>
        /// 建立连接时触发
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            //绑定用户id和连接id以便快速定位用户客户端
            var connid = Context.ConnectionId;
            var userid = Context.User.Claims.ToList()[0].Value;
            _cache.SetValue(prefix + userid, connid);
            await Clients.Client(connid).SendAsync("onConnected",userid,connid);
        }
        /// <summary>
        /// 丢失连接时触发
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var connid = Context.ConnectionId;
            var userid = Context.User.Claims.ToList()[0].Value;
            _cache.DelKey(prefix + userid);
            await Clients.Client(connid).SendAsync("onDisconnected");
        }
        /// <summary>
        /// 通知指定用户的客户端收到
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task SendMessageToClient(string userid, string message)
        {
            var connid = _cache.GetValue(prefix + userid);
            if (!string.IsNullOrEmpty(connid))
            {
                await Clients.Client(connid).SendAsync("messageReceived", message);
            }
        }
        /// <summary>
        /// 公共消息所有人可以收到
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async void SendPublicMessage(string message)
        {
            await Clients.All.SendAsync("publicMessageReceived", message);
        }
        /// <summary>
        /// 给多名用户推送消息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async void SendMessageToClients(string[] userids, string message)
        {
            foreach (var userid in userids)
            {
                var connid = _cache.GetValue(prefix + userid);
                if (!string.IsNullOrEmpty(connid))
                {
                    await Clients.Client(connid).SendAsync("messageReceivedM", message);
                }
            }
        }
    }
}