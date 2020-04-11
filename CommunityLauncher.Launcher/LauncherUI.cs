using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using CommunityLauncher;
using TaleWorlds.Engine;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.CustomWidgets;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;
using TaleWorlds.TwoDimension;

using Material = TaleWorlds.TwoDimension.Material;

namespace CommunityLauncher
{
}


    public class LauncherUI 
    {
       private Material _material;

		private TwoDimensionContext _twoDimensionContext;

		private UIContext _context;

		private GauntletMovie _movie;

		private CommunityLauncherVM _viewModel;

		private SpriteData _spriteData;

		private WidgetFactory _widgetFactory;

		private UserDataManager _userDataManager;

		private readonly Action _onClose;

		private readonly Action _onMinimize;

		public LauncherUI(UserDataManager userDataManager, UIContext context, Action onClose, Action onMinimize)
		{
			this._context = context;
			this._twoDimensionContext = this._context.TwoDimensionContext;
			this._userDataManager = userDataManager;
			this._onClose = onClose;
			this._onMinimize = onMinimize;
		}

		public void Initialize()
		{
			this._spriteData = this._context.SpriteData;
			this._spriteData.SpriteCategories["ui_launcher"].Load(this._twoDimensionContext.ResourceContext, this._twoDimensionContext.ResourceDepot);
			this._spriteData.SpriteCategories["ui_fonts"].Load(this._twoDimensionContext.ResourceContext, this._twoDimensionContext.ResourceDepot);
			this._material = new PrimitivePolygonMaterial(new Color(0.5f, 0.5f, 0.5f, 1f));
			this._widgetFactory = new WidgetFactory(this._context.ResourceDepot, "Prefabs");
			this._widgetFactory.PrefabExtensionContext.AddExtension(new PrefabDatabindingExtension());
			this._widgetFactory.Initialize();
			this._viewModel = new CommunityLauncherVM(this._userDataManager, this._onClose, this._onMinimize);
			this._movie = GauntletMovie.Load(this._context, this._widgetFactory, "LauncherUI", this._viewModel);
			this._viewModel.Init(this._movie);
		}

		public string AdditionalArgs
		{
			get
			{
				if (this._viewModel == null)
				{
					return "";
				}
				return this._viewModel.GameTypeArgument + " " + this._viewModel.ModsData.ModulesListCode;
			}
		}

		public void Update()
		{
			this._movie.Update();
			this._widgetFactory.CheckForUpdates();
		}

		public bool CheckMouseOverWindowDragArea()
		{
			return this._context.EventManager.HoveredView is LauncherDragWindowAreaWidget;
		}

		public bool HitTest()
		{
			return this._movie != null && this._context.HitTest(this._movie.RootView.Target);
		}
	
    }
