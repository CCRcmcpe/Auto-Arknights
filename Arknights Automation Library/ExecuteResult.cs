namespace REVUnit.AutoArknights.Core
{
    public class ExecuteResult
    {
        public string Message { get; set; }
        public bool Succeed { get; set; }

        public static ExecuteResult MaxRetryReached(int position)
        {
            return new ExecuteResult {Succeed = false, Message = $"在第{position}步未能点击目标"};
        }

        public static ExecuteResult Success(string message = "")
        {
            return new ExecuteResult {Succeed = true, Message = message};
        }
    }
}