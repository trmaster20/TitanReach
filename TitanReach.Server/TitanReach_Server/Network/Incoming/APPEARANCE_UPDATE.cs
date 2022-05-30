using System.Collections.Generic;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class APPEARANCE_UPDATE : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.PLAYER_CUSTOM_UPDATE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            packet.ReadByte();

            uint puid = packet.ReadUInt32();
            bool found = false;

            
            foreach (AccountCharacterData c in p.charData)
            {
                if(c != null)
                {
                    if(c.characterId == puid)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found) return;

            List<AppearanceData> allData = new List<AppearanceData>();
         
            AppearanceData data = new AppearanceData();
            data.slotType = "Gender";
            data.slotId = 0;
            data.clothingId = (int) packet.ReadByte();
            allData.Add(data);

            data = new AppearanceData();
            data.slotType = "Skin Color Index";
            data.slotId = 1;
            data.clothingId = packet.ReadByte();
            allData.Add(data);

            data = new AppearanceData();
            data.slotType = "Hair";
            data.slotId = 2;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);

            data = new AppearanceData();
            data.slotType = "Facial Hair";
            data.slotId = 3;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);
            
            data = new AppearanceData();
            data.slotType = "Torso";
            data.slotId = 4;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);          
            
            
            data = new AppearanceData();
            data.slotType = "Pants";
            data.slotId = 5;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);
            
            data = new AppearanceData();
            data.slotType = "Shoe";
            data.slotId = 6;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);

            data = new AppearanceData();
            data.slotType = "Gloves";
            data.slotId = 7;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data); 
            
            data = new AppearanceData();
            data.slotType = "FacePaint";
            data.slotId = 8;
            data.clothingId = packet.ReadByte();
            data.slotColor1 = packet.ReadByte();
            allData.Add(data);

            Server.Instance.DB.UpdateAppearance((int)puid, allData);

           // p.appearanceData = allData;

            //p.NetworkActions.PlayerCustomUpdate();


        }
    }
}
