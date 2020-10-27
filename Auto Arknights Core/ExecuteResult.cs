namespace REVUnit.AutoArknights.Core
{
    public class ExecuteResult
    {
        public string? Message { get; set; }
        public bool Succeed { get; set; }

        public static ExecuteResult MaxRetryReached(string operation, int triedCount) =>
            new ExecuteResult { Succeed = false, Message = $"尝试了{triedCount}次也未能达成{operation}操作" };

        public static ExecuteResult Success() => new ExecuteResult { Succeed = true, Message = "任务全部顺利完成" };
    }
}