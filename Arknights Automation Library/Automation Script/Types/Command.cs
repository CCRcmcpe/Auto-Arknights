using System.Drawing;
using System.Threading.Tasks;
using REVUnit.ImageLocator;

namespace REVUnit.AutoArknights.Library.Automation_Script
{
    public abstract class Command
    {
        public abstract void Execute();
        protected LocateResult Find(string asset)
        {
            var target = Assets.Get(asset);
            return Automation.I.FindPos(target);
        }
    }
}