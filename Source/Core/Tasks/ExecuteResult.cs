using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public class ExecuteResult
    {
        public ExecuteResult(bool successful, string? message = null)
        {
            Successful = successful;
            Message = message ?? Resources.ExecuteResult_Ended;
        }

        public bool Successful { get; set; }
        public string Message { get; set; }

        public static ExecuteResult MaxRetryReached(string operation, int triedCount)
        {
            return new(false, string.Format(Resources.ExecuteResult_MaxRetryReached, triedCount, operation));
        }

        public static ExecuteResult Success(string? message = null)
        {
            return new(true, message ?? Resources.ExecuteResult_Success);
        }
    }
}