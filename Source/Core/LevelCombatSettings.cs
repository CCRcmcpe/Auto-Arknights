using System.Text;

namespace REVUnit.AutoArknights.Core
{
    public class LevelCombatSettings
    {
        public int MaxOriginitesUsage { get; set; }
        public int RepeatTimes { get; set; }
        public bool UseOriginites { get; set; }
        public bool UseSanityPotions { get; set; }
        public bool WaitWhenNoSanity { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(RepeatTimes switch
            {
                -1 or 0 => "理智不足终止",
                _ => $"到达次数 {RepeatTimes} 终止"
            });
            if (WaitWhenNoSanity)
            {
                sb.Append(" 等待自然恢复");
            }

            if (UseOriginites)
            {
                sb.Append(MaxOriginitesUsage == -1 ? " 无限使用源石" : $" 使用最多{MaxOriginitesUsage}源石");
            }

            if (UseSanityPotions)
            {
                sb.Append(" 使用理智药剂");
            }

            return sb.ToString();
        }
    }
}