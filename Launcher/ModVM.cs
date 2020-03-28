using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using System.Windows.Forms;
using TaleWorlds.Library;
using Modio;
using Modio.Filters;
using Modio.Models;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.Lobby.HostGame.HostGameOptions;
using File = System.IO.File;
namespace CommunityLauncher
{
    public class DownloadModsVM : ViewModel
    {
        private MBBindingList<ModVM> _downloadableMods;
        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> SortBySelection  ;
        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> FilterBySelection;

        private CommunityLauncherVM communityLauncherVm;
        private Client _client;

        public DownloadModsVM(CommunityLauncherVM communityLauncherVm)
        {
            _client = new Client(new Credentials("f3312601170f0cbf46d0f786552402ef"/*, "token"*/));
            DownloadableMods = new MBBindingList<ModVM>();
            SortBySelection = new SelectorVM<SelectorItemVM>(new List<string>(){"Rating","Downloads","Popularity","Newest"},3,onSortChanged );
            this.communityLauncherVm = communityLauncherVm;
        }

        private void onSortChanged(SelectorVM<SelectorItemVM> obj)
        {
            
        }

        [DataSourceProperty]
        public MBBindingList<ModVM> DownloadableMods
        {
            get { return _downloadableMods; }
            set
            {
                if (value == _downloadableMods)
                    return;
                _downloadableMods = value;
                OnPropertyChanged(nameof(DownloadableMods));
            }
        }

        private void ChangeLoadingOrderOf(ModVM targetModule, int insertIndex)
        {
            var index = DownloadableMods.IndexOf(targetModule);
            DownloadableMods.RemoveAt(index);
            DownloadableMods.Insert(insertIndex, targetModule);
        }

        private async void ChangeIsSelectedOf(ModVM targetModule)
        {
            foreach (var mod in DownloadableMods)
            {
                if (!mod.IsSelected) continue;
                var dependencies = await  _client.Games[324].Mods[mod.Mod.Id].Dependencies.Get();
                foreach (Dependency dep in dependencies)
                {
                    var depMod = DownloadableMods.FirstOrDefault(x => x.Mod.Id == dep.ModId);
                    if (depMod != default)
                    {
                        depMod.IsSelected = true;
                    }
                }
            }
            communityLauncherVm.PlayText = string.Empty;
            communityLauncherVm.PlayText = DownloadableMods.Any(m => m.IsSelected) ? "Download & Install" : "P L A Y";

      ;
        }

        public async void Refresh()
        {
            
            var filter = ModFilter.Rating.Desc();
             var mods = await _client.Games[324].Mods.Search(filter).ToList();
            DownloadableMods.Clear();
            foreach (var mod in mods)
            {
                DownloadableMods.Add(new ModVM(mod, ChangeLoadingOrderOf, ChangeIsSelectedOf));
            }
        }


        public async void Install()
        {
            communityLauncherVm.CanLaunch = false;
            communityLauncherVm.PlayText = "Downloading";
            

            foreach (var mod in DownloadableMods)
            {
                if (!mod.IsSelected) continue;
                
                await DownloadMod(mod); 
            }

            communityLauncherVm.ModsData.Refresh(communityLauncherVm.IsMultiplayer);
            communityLauncherVm.PlayText = "P L A Y";
            communityLauncherVm.CanLaunch = true;
        }

        private  async Task DownloadMod(ModVM mod)
        {
            
            var client = new HttpClient();
            var zipPath = string.Format(BasePath.Name + "Modules/{0}", mod.Mod.Modfile.Filename);
            File.Delete(zipPath);
            var response = await client.GetAsync(mod.Mod.Modfile.Download.BinaryUrl);
            using (var fs = new FileStream(
                zipPath,
                FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }

            if (!DeleteOldMod(mod, zipPath))
            {
                DeleteModZip(zipPath);
                return;
            }
            ExtractModZip(zipPath);
            DeleteModZip(zipPath);
        }

        private bool DeleteOldMod(ModVM mod, string zipPath)
        {
            var zipFile = ZipFile.OpenRead(zipPath);
            var listOfZipFolders = zipFile.Entries.Where(x => x.FullName.EndsWith("/")).ToList();
            zipFile.Dispose();
            var updatedAllFiles = true;
            foreach (var modulePath in listOfZipFolders.Select(folder => $"{BasePath.Name}/Modules/{folder}").Where(Directory.Exists))
            {
                if (MessageBox.Show($"Mod: {mod.Name} Remove Folder to Update mod? {modulePath}", "Delete Folder?",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    updatedAllFiles = false;
                }
                else
                {
                    Directory.Delete(modulePath, true);
                }
            }
            if(!updatedAllFiles )
            {
                MessageBox.Show($"User cancelled Installation of Mod: {mod.Name}", "Mod not Installed",
                    MessageBoxButtons.OK);

            }
            return updatedAllFiles;
        }

        private  void DeleteModZip(string zipPath)
        {
            File.Delete(zipPath);
        }

        private  void ExtractModZip(string zipPath)
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
        public string Summary => _mod.Summary;

        [DataSourceProperty]
        public string Logo => _mod.Logo.Original.AbsoluteUri;

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

        public ModVM(Mod mod, Action<ModVM, int> onChangeLoadingOrder, Action<ModVM> onSelect)
        {
            _onChangeLoadingOrder = onChangeLoadingOrder;
            _onSelect = onSelect;
            _mod = mod;
        }
    }
}