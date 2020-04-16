using System;
using TaleWorlds.Library;
using Modio.Models;

namespace CommunityLauncher.Launcher.ViewModels
{
    public class ModVM : ViewModel
    {
        private Mod _mod;

        private readonly Action<ModVM> _onSelect;
        private bool _isSelected;
        private bool _isDisabled;

        [DataSourceProperty]
        public string Name
        {
            get { return _mod.Name; }
        }

        public Mod Mod => _mod;

        [DataSourceProperty]
        public string Description
        {
            get { return _mod.Description; }
        }

        [DataSourceProperty] public string Summary => _mod.Summary;

        [DataSourceProperty] public string Logo => _mod.Logo.Original.AbsoluteUri;

        [DataSourceProperty]
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (value != _isDisabled)
                {
                    _isDisabled = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    _onSelect(this);
                }
            }
        }

        public ModVM(Mod mod, Action<ModVM> onSelect)
        {
            _onSelect = onSelect;
            _mod = mod;
        }
    }
}