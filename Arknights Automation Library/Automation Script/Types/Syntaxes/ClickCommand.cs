using System.Threading.Tasks;
using REVUnit.ImageLocator;

namespace REVUnit.AutoArknights.Library.Automation_Script
{
    public class ClickCommand : Command
    {
        public Identfier Target { get; set; }
        
        public override void Execute()
        {
            var asset = Target.Value;
            var result = Find(asset);
            if (result.Succeed)
            {
                result.CenterPoint.Click();
            }
        }
    }
}