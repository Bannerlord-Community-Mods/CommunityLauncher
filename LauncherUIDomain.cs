using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher;
using TaleWorlds.MountAndBlade.Launcher.UserDatas;
using TaleWorlds.TwoDimension;
using TaleWorlds.TwoDimension.Standalone;

namespace CommunityLauncher
{
	internal class StandaloneInputService : IInputService
	{
		public StandaloneInputService(GraphicsForm graphicsForm)
		{
		}

		bool IInputService.MouseEnabled
		{
			get
			{
				return true;
			}
		}

		bool IInputService.KeyboardEnabled
		{
			get
			{
				return true;
			}
		}

		bool IInputService.GamepadEnabled
		{
			get
			{
				return false;
			}
		}
	}
    public class LauncherUIDomain: FrameworkDomain
    {
   
		private bool _initialized;

		private GraphicsForm _graphicsForm;

		private GraphicsContext _graphicsContext;

		private UIContext _gauntletUIContext;

		private TwoDimensionContext _twoDimensionContext;

		private RPLauncherUI _launcherUI;

		private readonly ResourceDepot _resourceDepot;

		public UserDataManager UserDataManager { get; private set; }

		public LauncherUIDomain(GraphicsForm graphicsForm, ResourceDepot resourceDepot)
		{
			this._graphicsForm = graphicsForm;
			this._resourceDepot = resourceDepot;
			this.UserDataManager = new UserDataManager();
		}

		public override void Update()
		{
			if (!this._initialized)
			{
				this.UserDataManager.LoadUserData();
				Input.Initialize(new StandaloneInputManager(this._graphicsForm), null);
				this._graphicsForm.InitializeGraphicsContext(this._resourceDepot);
				this._graphicsContext = this._graphicsForm.GraphicsContext;
				TwoDimensionPlatform twoDimensionPlatform = new TwoDimensionPlatform(this._graphicsForm);
				this._twoDimensionContext = new TwoDimensionContext(twoDimensionPlatform, twoDimensionPlatform, this._resourceDepot);
				StandaloneInputService inputService = new StandaloneInputService(this._graphicsForm);
				InputContext inputContext = new InputContext();
				inputContext.MouseOnMe = true;
				inputContext.IsKeysAllowed = true;
				inputContext.IsMouseButtonAllowed = true;
				inputContext.IsMouseWheelAllowed = true;
				this._gauntletUIContext = new UIContext(this._twoDimensionContext, inputContext, inputService);
				this._gauntletUIContext.IsDynamicScaleEnabled = false;
				this._gauntletUIContext.Initialize();
				this._launcherUI = new RPLauncherUI(this.UserDataManager, this._gauntletUIContext, new Action(this.OnCloseRequest), new Action(this.OnMinimizeRequest));
				this._launcherUI.Initialize();
				this._initialized = true;
			}
			bool mouseOverDragArea = this._launcherUI.CheckMouseOverWindowDragArea();
			this._graphicsForm.UpdateInput(mouseOverDragArea);
			this._graphicsForm.BeginFrame();
			Input.Update();
			this._graphicsForm.Update();
			this._gauntletUIContext.UpdateInput(InputType.MouseButton | InputType.MouseWheel | InputType.Key);
			this._gauntletUIContext.Update(0.0166666675f);
			this._launcherUI.Update();
			this._gauntletUIContext.LateUpdate(0.0166666675f);
			this._graphicsForm.PostRender();
			this._graphicsContext.SwapBuffers();
		}

		public string AdditionalArgs
		{
			get
			{
				if (this._launcherUI == null)
				{
					return "";
				}
				return this._launcherUI.AdditionalArgs;
			}
		}

		public override void Destroy()
		{
			this._initialized = false;
			this._graphicsContext.DestroyContext();
			this._gauntletUIContext = null;
			this._launcherUI = null;
			this._graphicsForm.Destroy();
		}

		private void OnStartGameRequest()
		{
		}

		private void OnCloseRequest()
		{
			Environment.Exit(0);
		}

		private void OnMinimizeRequest()
		{
			this._graphicsForm.MinimizeWindow();
		}
	}
    
}