using Microsoft.Extensions.Logging;
using ZHWB.Infrastructure.MessageQueue;
namespace ZHWB.Infrastructure.Log
{
    public class ExceptionLogger:IExceptionLogger
    {
        private ILogger _logger;
        private IRabitMQHandler _mqHandler;
        public ExceptionLogger(ILogger<ExceptionLogger> logger,IRabitMQHandler mQHandler){
            _logger=logger;
            _mqHandler=mQHandler;
        }
        public void OnApiResolveException(string exmsg,string controller,string action){
             //_mqHandler.SendMQMessage("",Utils.ErrorLogKey.ErrorLogExchange,
             //Utils.ErrorLogKey.ErrorLogQueueName,Utils.ErrorLogKey.ErrorLogRoutingKey);
        }
        public void OnPageLoadException(string exmsg,string page){

        }
        public void OnDataSaveException(string exmsg,string sql)
        {

        }
    }
}