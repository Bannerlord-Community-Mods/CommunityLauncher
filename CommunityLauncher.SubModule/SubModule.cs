using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.View.Screen;
using Module = TaleWorlds.MountAndBlade.Module;

namespace CommunityLauncher.SubModule
{
    public sealed class SyncXMLServer : GameNetworkMessage
    {
        public string name;
        public string xmlfile;
        public SyncXMLServer(string name, string xmlfile)
        {
            this.name = name;
            this.xmlfile = xmlfile;
        }

        public SyncXMLServer()
        {
        }

        protected override bool OnRead()
        {
            bool result = true;
            name = ReadStringFromPacket(ref result);

            xmlfile = ReadStringFromPacket(ref result);
            
            return result;
        }

        protected override void OnWrite()
        {
            WriteStringToPacket(name);
          
            WriteStringToPacket(xmlfile);
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return $"Received XML File: {name}";
        }
    }

public class XMLSync : MissionNetwork
    {
        public override void AfterStart()
        {

            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            GameNetwork.AddNetworkHandler(this);

            base.AfterStart();
        }
        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register((new GameNetworkMessage.ClientMessageHandlerDelegate<SyncXMLServer>(HandleXMLFileClient)));
            networkMessageHandlerRegisterer.Register(
                (new GameNetworkMessage.ServerMessageHandlerDelegate<SyncXMLServer>(HandleXMLFileEventServer)));
        }



        public void HandleXMLFileEventServer(SyncXMLServer gameNetworkMessage)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(gameNetworkMessage.xmlfile);
            MBObjectManager.Instance.LoadXml(doc, null);
        }
        public bool HandleXMLFileClient(NetworkCommunicator peer, SyncXMLServer message)
        {

            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message.xmlfile);
            MBObjectManager.Instance.LoadXml(doc, null);
            return true;
        }

        // [CommandLineFunctionality.CommandLineArgumentFunction("sync_xml", "mp_host")]
        public static string MPHostHelp(List<string> strings)
        {
            var component = Mission.Current.GetMissionBehaviour<XMLSync>();
            if (component != null)
            {
                if (GameNetwork.IsServer && strings.Count > 0)
                {
                    var path = $"{BasePath.Name}Modules/CommunityLauncher/ModuleData/{strings[0]}";
                    if (!File.Exists(path))
                    {
                        return $"File does not exist: {path}";
                    }

                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SyncXMLServer(strings[0], File.ReadAllText(path)));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    return "Success";
                }

                return "Not enough arguments";

            }

            return "Component Missing";

        }

        public override void OnMissionTick(float dt)
        {

            base.OnMissionTick(dt);
        }

        protected override void OnUdpNetworkHandlerTick()
        {
            base.OnUdpNetworkHandlerTick();
        }

        protected override void OnUdpNetworkHandlerClose()
        {
            base.OnUdpNetworkHandlerClose();
        }
    }

    public class EnableDebugHotkeys : MissionView
    {
        public override void OnMissionTick(float dt)
        {
            if (!MissionScreen.SceneLayer.Input.IsCategoryRegistered(HotKeyManager.GetCategory("Debug")))
            {

                MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Debug"));
            }
            base.OnMissionTick(dt);
        }
    }

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

            toggle_imgui_console_visibility(new UIntPtr());
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
                var handler = mission.GetMissionBehaviour<EnableDebugHotkeys>();
                if (handler == null)
                {


                    mission.AddMissionBehaviour(new EnableDebugHotkeys());
                }
            }

            base.OnMissionBehaviourInitialize(mission);
        }
    }
}