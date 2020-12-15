namespace REVUnit.AutoArknights.Core.Tasks
{
    public class ExecuteResult
    {
        public ExecuteResult(bool succeed, string message = "任务结束")
        {
            Succeed = succeed;
            Message = message;
        }

        public bool Succeed { get; set; }
        public string? Message { get; set; }

        public static ExecuteResult MaxRetryReached(string operation, int triedCount) =>
            new(false, $"尝试了{triedCount}次也未能达成{operation}操作");

        public static ExecuteResult Success(string message = "任务完成") => new(true, message);
    }
}