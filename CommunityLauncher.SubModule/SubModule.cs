using CommunityLauncher.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using CommunityLauncher.Submodule;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using Module = TaleWorlds.MountAndBlade.Module;

namespace CommunityLauncher.SubModule
{
    public class SubModule : MBSubModuleBase
    {
        [DllImport("Rgl.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?toggle_imgui_console_visibility@rglCommand_line_manager@@QEAAXXZ")]
        public static extern void toggle_imgui_console_visibility(UIntPtr x);
        protected override void OnSubModuleLoad()
        {
            var x = AppDomain.CurrentDomain;
            x.UnhandledException += (x, e) => {

#if NETSTANDARD
                Console.WriteLine(e.ExceptionObject.ToString());
#else
                Exception ex = (Exception) e.ExceptionObject;
                ErrorWindow.Display(ex.Message, ex.Source, ex.StackTrace);
                Console.WriteLine(e.ExceptionObject.ToString());
#endif
            };

            //toggle_imgui_console_visibility(new UIntPtr());
            Debug.DebugManager = new HTMLDebugManager();
            base.OnSubModuleLoad();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            typeof(Module).GetField("_splashScreenPlayed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Module.CurrentModule, true);
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {

            base.OnGameLoaded(game, initializerObject);
        }

        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            // ReloadMPCLasses();
            base.OnMultiplayerGameStart(game, starterObject);
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("xml", "mp_host")]
        private static string ReloadMPCLasses(List<string> strings)
        {
            if (strings.Count != 1) return "Invalid Arguments";
            Type type;
            string filename;
            switch (strings[0])
            {
                case "MPHeroClass":
                    type = typeof(MultiplayerClassDivisions.MPHeroClass);
                    filename = "mpclassdivisions.xml";
                    break;
                case "BasicCultureObject":
                    type = typeof(BasicCultureObject);
                    filename = "mpcultures.xml";
                    break;
                case "BasicCharacterObject":
                    type = typeof(BasicCharacterObject);
                    filename = "mpcharacters.xml";
                    break;
                case "Items":
                    type = typeof(ItemObject);
                    filename = "mpitems.xml";
                    break;
                case "CraftingPiece":
                    type = typeof(CraftingPiece);
                    filename = "crafting_pieces.xml";
                    break;
                default:
                    return $"Invalid Type: {strings[0]}";
            }

            var filePath = $"{BasePath.Name}Modules/CommunityLauncher/ModuleData/{filename}";
            if (!File.Exists(filePath))
            {
                return $"File not Found {filePath}";
            }

            MBObjectManager.Instance.ClearAllObjectsWithType(type);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            MBObjectManager.Instance.LoadXml(doc, null);
            return $"Reloaded {filename}";
        }

        protected override void OnApplicationTick(float dt)
        {
            //var x = Input.DebugInput;
            base.OnApplicationTick(dt);
            /* if (Input.IsKeyReleased(InputKey.Numpad5))
             {
                 ReloadMPCLasses();
             }*/
        }

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            // @todo: HERE
            if (GameNetwork.IsServer)
            {
                var handler = mission.GetMissionBehaviour<EnableDebugHotkeysMissionView>();
                if (handler == null)
                {
                    mission.AddMissionBehaviour(new EnableDebugHotkeysMissionView());
                }
            }

            base.OnMissionBehaviourInitialize(mission);
        }
    }
}
