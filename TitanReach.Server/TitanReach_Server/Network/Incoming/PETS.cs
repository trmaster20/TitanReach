using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class PETS : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.PET;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subType = packet.ReadByte();


            switch (subType)
            {
                //SPAWN
                case 2:
                    int petID = packet.ReadByte();
                    p.PetManager.SetActivePet(petID);
                    break;
                //DESTROY
                case 3:
                    foreach (Player pl in p.Map.Players) {
                        pl.NetworkActions.DestroyPet(p);
                    }
                    break;
            }
        }
    }
}
