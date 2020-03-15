using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReactiveUI;
using REVUnit.AutoArknights.Properties;

namespace REVUnit.AutoArknights.ViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        public IReadOnlyList<PropertyViewModel> Properties =>
            Settings.Default.Properties.Cast<SettingsProperty>()
                .Select(p => new PropertyViewModel(Settings.Default, p)).ToArray();
    }
}