using ReactiveUI;
using REVUnit.AutoArknights.GUI.Core;

namespace REVUnit.AutoArknights.GUI.ViewModels
{
    public abstract class JobViewModel : ReactiveObject
    {
        public Job Model { get; set; }
    }
}