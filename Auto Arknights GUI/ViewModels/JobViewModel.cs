using ReactiveUI;
using REVUnit.AutoArknights.Core;

namespace REVUnit.AutoArknights.GUI.ViewModels
{
    public abstract class JobViewModel : ReactiveObject
    {
        public Job Model { get; set; }
    }
}