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
using System.Threading.Tasks;
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
	internal class Program
    {
		private static WindowsFramework _windowsFramework;

		private static GraphicsForm _graphicsForm;

		private static List<string> _args;

		private static string _additioanlArgs;

		private static LauncherUIDomain _standaloneUIDomain;

		private static bool _gameStarted;

		private static async Task Main(string[] args)
		{


			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
			CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
			var resourceDepot = new ResourceDepot(BasePath.Name);
		
			resourceDepot.AddLocation("GUI/Launcher/");
			resourceDepot.AddLocation("Modules/Native/LauncherGUI/");
			
			resourceDepot.AddLocation("Modules/CommunityLauncher/GUI/");
			resourceDepot.CollectResources();
			resourceDepot.StartWatchingChangesInDepot(); 
			Program._args = args.ToList<string>();
			var name = "M&B II: Bannerlord Community Launcher";
			Program._graphicsForm = new GraphicsForm(1154, 701, resourceDepot, true, true, true, name);
			Program._windowsFramework = new WindowsFramework {ThreadConfig = WindowsFrameworkThreadConfig.SingleThread};
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

		public static void StartGame()
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