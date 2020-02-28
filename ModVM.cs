using System.Net.Http;
using CommunityLauncher.ModIO;
using TaleWorlds.Library;


namespace CommunityLauncher
{
    public class ModsVM : ViewModel
    {
        private MBBindingList<ModVM> _mods;
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

        public  void Refresh()
        {
            var httpClient = new HttpClient();
            var result =
                httpClient.GetStringAsync(
                    "https://api.test.mod.io/v1/games/436/mods?api_key=7f33236a5397c6ff5288cea9182f6c89").Result;
            var modList = CommunityLauncher.ModIO.ModList.FromJson(result);
            this.Mods.Clear();
            foreach (var mod in modList.Data)
            {
                this.Mods.Add(new ModVM(mod));
            }
			

        }
        public ModsVM()
        {
           this.Mods = new MBBindingList<ModVM>();
        }
        
    }
    public class ModVM : ViewModel
    {
        private Mod _mod;

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return this._mod.Name;
            } 
        }

        [DataSourceProperty] public string Description{
            get
            {
                return this._mod.Description;
            } 
        } 
        
        [DataSourceProperty] public string Logo{
            get
            {
                return this._mod.Logo.Original.AbsoluteUri;
            } 
        }
        public ModVM(Mod mod)
        {
            this._mod = mod;
        }
    }
}