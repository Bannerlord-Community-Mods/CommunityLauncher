using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using CommunityLauncher.ModIO;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;


namespace CommunityLauncher
{
    public class ModsVM : ViewModel
    {
        private MBBindingList<ModVM> _mods;
        private CommunityLauncherVM communityLauncherVm;

        public ModsVM(CommunityLauncherVM communityLauncherVm)
        {
            this.Mods = new MBBindingList<ModVM>();
           
            this.communityLauncherVm = communityLauncherVm;
        }
        [DataSourceProperty]
        public MBBindingList<ModVM> Mods
        {
            get
            {
                return this._mods;
            }
            set
            {
                if (value == this._mods)
                    return;
                this._mods = value;
                this.OnPropertyChanged(nameof (Mods));
            }
        }
        private void ChangeLoadingOrderOf(ModVM targetModule, int insertIndex)
        {
            int index = this.Mods.IndexOf(targetModule);
            this.Mods.RemoveAt(index);
            this.Mods.Insert(insertIndex, targetModule);
        }

        private void ChangeIsSelectedOf(ModVM targetModule)
        {
            this.communityLauncherVm.PlayText = string.Empty;
            this.communityLauncherVm.PlayText =  Mods.Any(m => m.IsSelected) ? "Download & Play" : "P L A Y";
           
        }

        public  void Refresh()
        {
            var httpClient = new HttpClient();
            var result =
                httpClient.GetStringAsync(
                    "https://api.test.mod.io/v1/games/436/mods?_sort=-rating&api_key=7f33236a5397c6ff5288cea9182f6c89").Result;
            var modList = CommunityLauncher.ModIO.ModList.FromJson(result);
            this.Mods.Clear();
            foreach (var mod in modList.Data)
            {
                this.Mods.Add(new ModVM(mod,new Action<ModVM, int>(this.ChangeLoadingOrderOf),new Action<ModVM>(this.ChangeIsSelectedOf) ));
            }
			

        }




        public async void  Install()
        {
            HttpClient client = new HttpClient();
            foreach (var mod in Mods)
            {
                if (mod.IsSelected)
                {
                    var response = await client.GetAsync(mod.Mod.Modfile.Download.BinaryUrl);
                    using (var fs = new FileStream(
                        string.Format(BasePath.Name+"Modules/{0}", mod.Mod.Modfile.Filename), 
                        FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                        fs.Close();
                        ZipFile.ExtractToDirectory(string.Format(BasePath.Name+"Modules/{0}", mod.Mod.Modfile.Filename),BasePath.Name+"Modules/");
                        this.communityLauncherVm.DlcData.Refresh(this.communityLauncherVm.IsMultiplayer);
                        this.communityLauncherVm.ModsData.Refresh(this.communityLauncherVm.IsMultiplayer);
                    }
                }
            }

            
                
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
            get
            {
                return this._mod.Name;
            } 
        }
        public Mod  Mod => this._mod;

        [DataSourceProperty] public string Description{
            get
            {
                return this._mod.Description;
            } 
        } 
        [DataSourceProperty] public string Summary{
            get
            {
                return this._mod.Summary;
            } 
        } 
        [DataSourceProperty] public string Logo{
            get
            {
                return this._mod.Logo.Original.AbsoluteUri;
            } 
        }
        [DataSourceProperty]
        public bool IsDisabled
        {
            get
            {
                return this._isDisabled;
            }
            set
            {
                if (value != this._isDisabled)
                {
                    this._isDisabled = value;
                    base.OnPropertyChanged("IsDisabled");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
               
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChanged("IsSelected");
                    this._onSelect(this);
                }
            }
        }
        public ModVM(Mod mod,Action<ModVM, int> onChangeLoadingOrder, Action<ModVM> onSelect)
        {
            this._onChangeLoadingOrder = onChangeLoadingOrder;
            this._onSelect = onSelect;
            this._mod = mod;
        }
    }
}