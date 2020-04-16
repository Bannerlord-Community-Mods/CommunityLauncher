using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace CommunityLauncher.Submodule
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
}
