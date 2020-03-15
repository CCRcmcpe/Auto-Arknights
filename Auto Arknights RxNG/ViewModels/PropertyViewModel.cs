using System;
using System.Configuration;
using ReactiveUI;

namespace REVUnit.AutoArknights.ViewModels
{
    public class PropertyViewModel : ReactiveObject
    {
        private readonly SettingsBase _parent;

        public PropertyViewModel(SettingsBase settings, SettingsProperty property)
        {
            Name = property.Name;
            _parent = settings;
            Model = property;
        }

        public SettingsProperty Model { get; set; }

        public string Name { get; }

        public string Value
        {
            get => _parent[Name].ToString();
            set
            {
                _parent[Name] = Convert.ChangeType(value, Model.PropertyType);
                _parent.Save();
            }
        }
    }
}