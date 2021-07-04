namespace REVUnit.AutoArknights.Core.Tasks
{
    public class ExecuteResult
    {
        public ExecuteResult(bool successful, string? message = null)
        {
            Successful = successful;
            Message = message ?? "任务结束";
        }

        public string Message { get; set; }

        public bool Successful { get; set; }

        public static ExecuteResult MaxRetryReached(string operation, int triedCount)
        {
            return new(false, $"尝试了{triedCount}次也未能达成{operation}操作");
        }

        public static ExecuteResult Success(string? message = null)
        {
            return new(true, message ?? "任务完成");
        }
    }
}