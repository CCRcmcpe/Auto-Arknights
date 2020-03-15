using System.Reflection;
using ReactiveUI;
using Splat;

namespace REVUnit.AutoArknights
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        public App()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }
    }
}