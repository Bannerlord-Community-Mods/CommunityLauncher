﻿using Modio;
using Modio.Filters;
using Modio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using File = System.IO.File;

namespace CommunityLauncher.Launcher.ViewModels
{
    public class DownloadModsVM : ViewModel
    {
        private MBBindingList<ModVM> _downloadableMods;
        [DataSourceProperty] public SelectorVM<SelectorItemVM> SortBySelection;
        [DataSourceProperty] public SelectorVM<SelectorItemVM> FilterBySelection;

        private CommunityLauncherVM communityLauncherVm;
        private Client _client;

        public DownloadModsVM(CommunityLauncherVM communityLauncherVm)
        {
            _client = new Client(new Credentials("f3312601170f0cbf46d0f786552402ef"/*, "token"*/));
            DownloadableMods = new MBBindingList<ModVM>();
            SortBySelection =
                new SelectorVM<SelectorItemVM>(new List<string>() { "Rating", "Downloads", "Popularity", "Newest" }, 3,
                    onSortChanged);
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

        private async void ChangeIsSelectedOf(ModVM targetModule)
        {
            foreach (var mod in DownloadableMods)
            {
                if (!mod.IsSelected) continue;
                var dependencies = await _client.Games[324].Mods[mod.Mod.Id].Dependencies.Get();
                foreach (Dependency dep in dependencies)
                {
                    var depMod = DownloadableMods.FirstOrDefault(x => x.Mod.Id == dep.ModId);
                    depMod.IsSelected = true;
                }
            }
            communityLauncherVm.PlayText = string.Empty;
            communityLauncherVm.PlayText = DownloadableMods.Any(m => m.IsSelected) ? "Download & Install" : "P L A Y";
        }

        public async void Refresh()
        {
            var filter = ModFilter.Rating.Desc();
            var httpClient = new HttpClient();
            var mods = await _client.Games[324].Mods.Search(filter).ToList();
            DownloadableMods.Clear();
            foreach (var mod in mods)
            {
                DownloadableMods.Add(new ModVM(mod, ChangeIsSelectedOf));
            }
        }

        public async Task Install()
        {
            foreach (var mod in DownloadableMods)
            {
                if (!mod.IsSelected) continue;

                await DownloadMod(mod);
            }
        }

        private async Task DownloadMod(ModVM mod)
        {
            var zipPath = string.Format(BasePath.Name + "Modules/{0}", mod.Mod.Modfile.Filename);

            System.IO.File.Delete(zipPath);
            var fileInfo = new FileInfo(zipPath);
            await _client.Download(mod.Mod, fileInfo);

            try
            {
                var deleteOldMod = DeleteOldMod(mod, fileInfo);
                if (!deleteOldMod)
                {
                    DeleteModZip(fileInfo);
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not Delete Mod ZIP {e.Message}");
                return;
            }

            try
            {
                ExtractModZip(fileInfo);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not Extract Mod ZIP {e.Message}");
                return;
            }

            try
            {
                DeleteModZip(fileInfo);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not Delete Mod ZIP {e.Message}");
                return;
                throw;
            }
        }

        private bool DeleteOldMod(ModVM mod, FileSystemInfo zipPath)
        {
            var zipFile = ZipFile.OpenRead(zipPath.FullName);
            var listOfZipFolders = zipFile.Entries.Where(x => x.FullName.EndsWith("/")).ToList();
            zipFile.Dispose();
            var updatedAllFiles = true;
            foreach (var modulePath in listOfZipFolders.Select(folder => $"{BasePath.Name}/Modules/{folder}")
                .Where(Directory.Exists))
            {
                if (MessageBox.Show($"Mod: {mod.Name} Remove Folder to Update mod? {modulePath}", "Delete Folder?",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    updatedAllFiles = false;
                    break;
                }
                else
                {
                    Directory.Delete(modulePath, true);
                }
            }

            if (!updatedAllFiles)
            {
                MessageBox.Show($"User cancelled Installation of Mod: {mod.Name}", "Mod not Installed",
                    MessageBoxButtons.OK);
            }

            return updatedAllFiles;
        }

        private void DeleteModZip(FileInfo zipPath)
        {
            File.Delete(zipPath.FullName);
        }

        private void ExtractModZip(FileInfo zipPath)
        {
            ZipFile.ExtractToDirectory(zipPath.FullName, BasePath.Name + "Modules/");
        }
    }
}
