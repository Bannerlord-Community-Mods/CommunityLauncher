using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace CommunityLauncher.Submodule
{
    public class XMLSyncMissionNetwork : MissionNetwork
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
            var component = Mission.Current.GetMissionBehaviour<XMLSyncMissionNetwork>();
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
}
