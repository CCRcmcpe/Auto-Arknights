using System;

namespace REVUnit.AutoArknights.Core
{
    public class LevelCombatSettings
    {
        public LevelCombatSettings(int repeatTimes, bool waitWhenNoSanity, bool useSanityPotions,
            int useOriginitesCount)
        {
            RepeatTimes = repeatTimes;
            WaitWhenNoSanity = waitWhenNoSanity;
            UseSanityPotions = useSanityPotions;
            UseOriginitesCount = useOriginitesCount;
        }

        public int RepeatTimes { get; }
        public int UseOriginitesCount { get; }
        public bool UseSanityPotions { get; }
        public bool WaitWhenNoSanity { get; }

        public override string ToString()
        {
            return string.Concat(
                RepeatTimes switch
                {
                    -1 or 0 => "理智不足终止",
                    _ => $"到达次数 {RepeatTimes} 终止"
                },
                WaitWhenNoSanity
                    ? " 等待自然恢复"
                    : string.Empty,
                UseSanityPotions
                    ? " 使用理智药剂"
                    : string.Empty,
                UseOriginitesCount switch
                {
                    -1 => " 无限使用源石",
                    0 => " 不使用源石",
                    >0 => $" 使用最多{UseOriginitesCount}颗源石",
                    _ => throw new ArgumentOutOfRangeException()
                });
        }
    }
}