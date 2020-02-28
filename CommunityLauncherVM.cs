using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;

namespace CommunityLauncher
{
    public class CommunityLauncherVM : ViewModel
    {
        private UserDataManager _userDataManager;
        private readonly Action _onClose;

        private readonly Action _onMinimize;

        private bool _isInitialized;

        private bool _isMultiplayer;

        private bool _isSingleplayerAvailable;

        private LauncherNewsVM _news;

        private LauncherModsVM _dlcData;
		
        private ModsVM _getModsData;
		
        private LauncherModsVM _modsData;
		
        private string _playText;

        private string _singleplayerText;

        private string _multiplayerText;

        private string _newsText;

        private string _dlcText;

        private string _modsText;

        private string _versionText;

        public string GameTypeArgument
        {
            get
            {
                if (!this.IsMultiplayer)
                {
                    return "/singleplayer";
                }

                return "/multiplayer";
            }
        }

        public CommunityLauncherVM(UserDataManager userDataManager, Action onClose, Action onMinimize)
        {
            this._userDataManager = userDataManager;
            this._onClose = onClose;
            this._onMinimize = onMinimize;
            this.PlayText = "P L A Y";
            this.SingleplayerText = "Singleplayer";
            this.MultiplayerText = "Multiplayer";
            this.NewsText = "News";
            this.DlcText = "DLC";
            this.ModsText = "Mods";
            this.VersionText = ApplicationVersion.FromParametersFile().ToString();
            this.News = new LauncherNewsVM();
            this.DlcData = new LauncherModsVM(this._userDataManager.UserData, true);
            this.ModsData = new LauncherModsVM(this._userDataManager.UserData, false);
            this.GetModsData = new ModsVM(this);
            this.IsSingleplayerAvailable = this.GameModExists("Sandbox");
            this.IsMultiplayer = (!this.IsSingleplayerAvailable ||
                                  this._userDataManager.UserData.GameType == GameType.Multiplayer);

            this.Refresh();
            this._isInitialized = true;
        }



        private void UpdateAndSaveUserModsData(bool isMultiplayer)
        {
            UserData userData = this._userDataManager.UserData;
            UserGameTypeData userGameTypeData = isMultiplayer ? userData.MultiplayerData : userData.SingleplayerData;
            userGameTypeData.DlcDatas.Clear();
            userGameTypeData.ModDatas.Clear();
            foreach (LauncherModuleVM launcherModuleVM in this.DlcData.Modules)
            {
                userGameTypeData.DlcDatas.Add(new UserModData(launcherModuleVM.Info.Id, launcherModuleVM.IsSelected));
            }

            foreach (LauncherModuleVM launcherModuleVM2 in this.ModsData.Modules)
            {
                userGameTypeData.ModDatas.Add(new UserModData(launcherModuleVM2.Info.Id, launcherModuleVM2.IsSelected));
            }

            this._userDataManager.SaveUserData();
        }

        private bool GameModExists(string modId)
        {
            List<ModuleInfo> list = ModuleInfo.GetModules().ToList<ModuleInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == modId)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnBeforeGameTypeChange(bool preSelectionIsMultiplayer, bool newSelectionIsMultiplayer)
        {
            if (!this._isInitialized)
            {
                return;
            }

            this._userDataManager.UserData.GameType =
                (newSelectionIsMultiplayer ? GameType.Multiplayer : GameType.Singleplayer);
            this.UpdateAndSaveUserModsData(preSelectionIsMultiplayer);
        }

        private void ExecuteStartGame()
        {
            switch (this.PlayText)
            {
                case "Download & Install":
                    this.GetModsData.Install();
                    break;
                    default:
                        
                        this.UpdateAndSaveUserModsData(this.IsMultiplayer);
                        Program.StartRPGame();
                        break;
            }
        }

        private void ExecuteClose()
        {
            this.UpdateAndSaveUserModsData(this.IsMultiplayer);
            Action onClose = this._onClose;
            if (onClose == null)
            {
                return;
            }

            onClose();
        }

        private void ExecuteMinimize()
        {
            /*Action onMinimize = this._onMinimize;
            if (onMinimize == null)
            {
                return;
            }

            onMinimize();*/
            
            this.PlayText =  "Testttttt";
        }

        private void Refresh()
        {
            this.News.Refresh(this.IsMultiplayer);
            this.DlcData.Refresh(this.IsMultiplayer);
            this.ModsData.Refresh(this.IsMultiplayer);
            this.GetModsData.Refresh();
        }

        [DataSourceProperty]
        public bool IsSingleplayer
        {
            get { return !this._isMultiplayer; }
            set
            {
                if (this._isMultiplayer != !value)
                {
                    this.IsMultiplayer = !value;
                }
            }
        }

        [DataSourceProperty]
        public bool IsMultiplayer
        {
            get { return this._isMultiplayer; }
            set
            {
                if (this._isMultiplayer != value)
                {
                    this.OnBeforeGameTypeChange(this._isMultiplayer, value);
                    this._isMultiplayer = value;
                    base.OnPropertyChanged("IsMultiplayer");
                    base.OnPropertyChanged("IsSingleplayer");
                    this.Refresh();
                }
            }
        }

        [DataSourceProperty]
        public bool IsSingleplayerAvailable
        {
            get { return this._isSingleplayerAvailable; }
            set
            {
                if (value != this._isSingleplayerAvailable)
                {
                    this._isSingleplayerAvailable = value;
                    base.OnPropertyChanged("IsSingleplayerAvailable");
                }
            }
        }

        [DataSourceProperty]
        public string VersionText
        {
            get { return this._versionText; }
            set
            {
                if (value != this._versionText)
                {
                    this._versionText = value;
                    base.OnPropertyChanged("VersionText");
                }
            }
        }

        [DataSourceProperty]
        public LauncherNewsVM News
        {
            get { return this._news; }
            set
            {
                if (value != this._news)
                {
                    this._news = value;
                    base.OnPropertyChanged("News");
                }
            }
        }

        [DataSourceProperty]
        public LauncherModsVM DlcData
        {
            get { return this._dlcData; }
            set
            {
                if (value != this._dlcData)
                {
                    this._dlcData = value;
                    base.OnPropertyChanged("DlcData");
                }
            }
        }

        [DataSourceProperty]
        public LauncherModsVM ModsData
        {
            get { return this._modsData; }
            set
            {
                if (value != this._modsData)
                {
                    this._modsData = value;
                    base.OnPropertyChanged("ModsData");
                }
            }
        }
        [DataSourceProperty]
        public ModsVM GetModsData
        {
            get { return this._getModsData; }
            set
            {
                if (value != this._getModsData)
                {
                    this._getModsData = value;
                    base.OnPropertyChanged("GetModsData");
                }
            }
        }
        [DataSourceProperty]
        public string PlayText
        {
            get { return this._playText; }
            set
            {
                if (this._playText != value)
                {
                    this._playText = value;
                    base.OnPropertyChanged("PlayText");
                }
            }
        }

        [DataSourceProperty]
        public string SingleplayerText
        {
            get { return this._singleplayerText; }
            set
            {
                if (this._singleplayerText != value)
                {
                    this._singleplayerText = value;
                    base.OnPropertyChanged("SingleplayerText");
                }
            }
        }

        [DataSourceProperty]
        public string MultiplayerText
        {
            get { return this._multiplayerText; }
            set
            {
                if (this._multiplayerText != value)
                {
                    this._multiplayerText = value;
                    base.OnPropertyChanged("MultiplayerText");
                }
            }
        }

        [DataSourceProperty]
        public string NewsText
        {
            get { return this._newsText; }
            set
            {
                if (this._newsText != value)
                {
                    this._newsText = value;
                    base.OnPropertyChanged("NewsText");
                }
            }
        }

        [DataSourceProperty]
        public string DlcText
        {
            get { return this._dlcText; }
            set
            {
                if (this._dlcText != value)
                {
                    this._dlcText = value;
                    base.OnPropertyChanged("DlcText");
                }
            }
        }

        [DataSourceProperty]
        public string ModsText
        {
            get { return this._modsText; }
            set
            {
                if (this._modsText != value)
                {
                    this._modsText = value;
                    base.OnPropertyChanged("ModsText");
                }
            }
        }
    }
}