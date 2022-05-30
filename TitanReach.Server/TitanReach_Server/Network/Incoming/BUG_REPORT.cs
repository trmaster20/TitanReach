using System;
using System.Runtime.Versioning;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class BUG_REPORT : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.BUG_REPORT;
        }

        public async void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            try
            {
                Response res = await Server.Instance.DB.SendBugReport(p, packet.ReadString(packet.ReadInt16()), packet.ReadString(packet.ReadInt16()), packet.ReadString(packet.ReadInt16()), packet.ReadString(packet.ReadInt16()));
                if (!res.Error)
                {
                    BugReportResponse msg = res.value as BugReportResponse;
                    p.Msg(msg.message);
                }
                else
                {
                    string error = null;

                    if (res.ErrorObject == null)
                        error = "Unknown Error";
                    else
                        error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error;

                    if (error != null)
                    {
                        Server.ErrorDB("[Bug Report Panel] Error from AID: " + p.UID + ": " + error);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Server.Error(p.Describe() + " " + e.Message + " - " + e.StackTrace);
                p.Disconnect("Error handling packets");
            }
        }
    }
}
