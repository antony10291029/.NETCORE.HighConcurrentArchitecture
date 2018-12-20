namespace ZHWB.Infrastructure.Log
{
    public interface IExceptionLogger
    {
        void OnApiResolveException(string exmsg,string controller,string action);
        void OnPageLoadException(string exmsg,string page);
        void OnDataSaveException(string exmsg,string sql);
    }
}