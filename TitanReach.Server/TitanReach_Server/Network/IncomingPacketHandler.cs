using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network
{
    public interface IncomingPacketHandler
    {

        public int GetID();

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet);
    }
}
