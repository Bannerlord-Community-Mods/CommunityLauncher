using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityLauncher.ModIO;
using TaleWorlds.Library;

namespace CommunityLauncher
{
    public class ModsVM : ViewModel
    {
        private MBBindingList<ModVM> _mods;
        private CommunityLauncherVM communityLauncherVm;

        public ModsVM(CommunityLauncherVM communityLauncherVm)
        {
            Mods = new MBBindingList<ModVM>();

            this.communityLauncherVm = communityLauncherVm;
        }

        [DataSourceProperty]
        public MBBindingList<ModVM> Mods
        {
            get { return _mods; }
            set
            {
                if (value == _mods)
                    return;
                _mods = value;
                OnPropertyChanged(nameof(Mods));
            }
        }

        private void ChangeLoadingOrderOf(ModVM targetModule, int insertIndex)
        {
            int index = Mods.IndexOf(targetModule);
            Mods.RemoveAt(index);
            Mods.Insert(insertIndex, targetModule);
        }

        private void ChangeIsSelectedOf(ModVM targetModule)
        {
            communityLauncherVm.PlayText = string.Empty;
            communityLauncherVm.PlayText = Mods.Any(m => m.IsSelected) ? "Download & Install" : "P L A Y";
        }

        public void Refresh()
        {
            var httpClient = new HttpClient();
            var result =
                httpClient.GetStringAsync(
                        "https://api.mod.io/v1/games/324/mods?_sort=-rating&api_key=f3312601170f0cbf46d0f786552402ef")
                    .Result;
            var modList = ModList.FromJson(result);
            Mods.Clear();
            foreach (var mod in modList.Data)
            {
                Mods.Add(new ModVM(mod, ChangeLoadingOrderOf, ChangeIsSelectedOf));
            }
        }


        public async void Install()
        {
            communityLauncherVm.CanLaunch = false;
            communityLauncherVm.PlayText = "Downloading";
            foreach (var mod in Mods)
            {
                if (!mod.IsSelected) continue;
                await DownloadMod(mod);
            }

            communityLauncherVm.DlcData.Refresh(communityLauncherVm.IsMultiplayer);
            communityLauncherVm.ModsData.Refresh(communityLauncherVm.IsMultiplayer);
            communityLauncherVm.PlayText = "P L A Y";
            communityLauncherVm.CanLaunch = true;
        }

        private static async Task DownloadMod(ModVM mod)
        {
            
            HttpClient client = new HttpClient();
            var zipPath = string.Format(BasePath.Name + "Modules/{0}", mod.Mod.Modfile.Filename);
            var response = await client.GetAsync(mod.Mod.Modfile.Download.BinaryUrl);
            using (var fs = new FileStream(
                zipPath,
                FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }

            ExtractModZip(zipPath);
            DeleteModZip(zipPath);
        }

        private static void DeleteModZip(string zipPath)
        {
            File.Delete(zipPath);
        }

        private static void ExtractModZip(string zipPath)
        {
            ZipFile.ExtractToDirectory(zipPath, BasePath.Name + "Modules/");
        }
    }

    public class ModVM : ViewModel
    {
        private Mod _mod;

        private readonly Action<ModVM, int> _onChangeLoadingOrder;

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

        [DataSourceProperty]
        public string Summary
        {
            get { return _mod.Summary; }
        }

        [DataSourceProperty]
        public string Logo
        {
            get { return _mod.Logo.Original.AbsoluteUri; }
        }

        [DataSourceProperty]
        public bool IsDisabled
        {
            get { return _isDisabled; }
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

        public ModVM(Mod mod, Action<ModVM, int> onChangeLoadingOrder, Action<ModVM> onSelect)
        {
            _onChangeLoadingOrder = onChangeLoadingOrder;
            _onSelect = onSelect;
            _mod = mod;
        }
    }
}