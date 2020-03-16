using System.Collections.Generic;

namespace REVUnit.AutoArknights.GUI.Core
{
    public class LevelPath
    {
        public LevelPath(string expression)
        {
            Expression = expression;
        }

        public IEnumerable<string> AssetExpressions { get; }

        public string Expression { get; }
    }
}