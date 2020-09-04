namespace REVUnit.AutoArknights.Core
{
    public class Sanity
    {
        public readonly int Max;
        public readonly int Value;

        public Sanity(int value, int max)
        {
            Value = value;
            Max = max;
        }

        public override string ToString()
        {
            return $"{Value}/{Max}";
        }
    }
}