using RabbitMQ.Client;
namespace ZHWB.Infrastructure.MessageQueue
{
    public interface IRabitMQHandler
    {
        int GetMessageCount(IConnection connection, string QueueName);
        string SendMsg(string exchange, string queueName, string routingKey, string msg, bool durable = false);
    }
}