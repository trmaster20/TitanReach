using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class CREATE_CHARACTER : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.CHARACTER_CREATOR;
        }

        public async void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            try
            {
                int subtype = packet.ReadByte();
                string name = packet.ReadString(packet.ReadByte());
                Response res = await Server.Instance.DB.CreateCharacter((int)p.AccountID, name);
                var thing = res.value as AccountData;
                string error = null;


                if (res.Error)
                {
                    if (res.ErrorObject == null)
                        error = "Unknown Error";
                    else
                        error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error;
                }

                if (error != null)
                {
                    p.NetworkActions.SendLoginResult(2, error);
                    Server.ErrorDB("Error from AID: " + p.UID + ": " + error);
                    return;
                }
                p.AccountID = (uint)thing.pId;
                p.UID = (uint)thing.pId;
                p.charData = thing.characters.ToArray();
                p.NetworkActions.SendLoginResult(LOGIN_OK);
            }
            catch (Exception e)
            {
                Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                p.Disconnect("Error handling packets");
            }
        }

        public int LOGIN_OK = 0;
    }
}
