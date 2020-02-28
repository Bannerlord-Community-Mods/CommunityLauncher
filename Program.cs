using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Launcher;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.TwoDimension.Standalone;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace CommunityLauncher
{
	internal static class MBDotNet
	{
		public const string MainDllName = "Rgl.dll";
		public const string DotNetLibraryDllName = "FairyTale.DotNet.dll";

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Rgl.dll", EntryPoint = "WotsMain", CallingConvention = CallingConvention.StdCall)]
		public static extern int WotsMainDotNet(string args);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("FairyTale.DotNet.dll", EntryPoint = "pass_controller_methods", CallingConvention = CallingConvention.StdCall)]
		public static extern void PassControllerMethods(Delegate currentDomainInitializer);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("FairyTale.DotNet.dll", EntryPoint = "pass_managed_initialize_method_pointer", CallingConvention = CallingConvention.StdCall)]
		public static extern void PassManagedInitializeMethodPointerDotNet([MarshalAs(UnmanagedType.FunctionPtr)] Delegate initalizer);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("FairyTale.DotNet.dll", EntryPoint = "pass_managed_library_callback_method_pointers", CallingConvention = CallingConvention.StdCall)]
		public static extern void PassManagedEngineCallbackMethodPointersDotNet([MarshalAs(UnmanagedType.FunctionPtr)] Delegate methodDelegate);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int SetCurrentDirectory(string args);
	}
	class NopGame : GameType
	{
		protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
			=> nextState = GameTypeLoadingStates.None;
		public override void OnDestroy() {}
	}
    internal class Program
    {
		private static WindowsFramework _windowsFramework;

		private static GraphicsForm _graphicsForm;

		private static List<string> _args;

		private static string _additioanlArgs;

		private static LauncherUIDomain _standaloneUIDomain;

		private static bool _gameStarted;
		private delegate void ControllerDelegate(Delegate currentDomainInitializer);

		private delegate void InitializerDelegate(Delegate argument);

		private delegate void StartMethodDelegate(string args);
		private static void Main(string[] args)
		{

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
			ResourceDepot resourceDepot = new ResourceDepot(BasePath.Name);
			resourceDepot.AddLocation("Modules/CommunityLauncher/GUI/GauntletUI/");
			resourceDepot.AddLocation("Modules/CommunityLauncher/GUI/");
			resourceDepot.CollectResources();
			Program._args = args.ToList<string>();
			string name = "M&B II: Bannerlord Community Launcher";
			Program._graphicsForm = new GraphicsForm(1154, 701, resourceDepot, true, true, true, name);
			Program._windowsFramework = new WindowsFramework();
			Program._windowsFramework.ThreadConfig = WindowsFrameworkThreadConfig.SingleThread;
			Program._standaloneUIDomain = new LauncherUIDomain(Program._graphicsForm, resourceDepot);
			Program._windowsFramework.Initialize(new FrameworkDomain[]
			{
				Program._standaloneUIDomain
			});
			Program._windowsFramework.RegisterMessageCommunicator(Program._graphicsForm);
			Program._windowsFramework.Start();
			if (Program._gameStarted)
			{
				ManagedStarter.Program.Main(Program._args.ToArray());
			}
		}

		#region dostuffwithoutlaunch
		
		

		#endregion

		public static void StartRPGame()
		{
			Program._additioanlArgs = Program._standaloneUIDomain.AdditionalArgs;
			Program._args.Add(Program._additioanlArgs);
			Program._gameStarted = true;
			Program._windowsFramework.UnRegisterMessageCommunicator(Program._graphicsForm);
			Program._graphicsForm.Destroy();
			Program._windowsFramework.Stop();
			Program._windowsFramework = null;
			Program._graphicsForm = null;
			User32.SetForegroundWindow(Kernel32.GetConsoleWindow());
			
		}
    }
}