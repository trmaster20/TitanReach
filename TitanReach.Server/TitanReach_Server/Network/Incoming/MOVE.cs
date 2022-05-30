using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{
    class MOVE : IncomingPacketHandler
    {
        public int GetID()
        {
            return Packets.MOVE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet) {
            byte subType = packet.ReadByte();
            if (subType == 0) // normal move
            {
                byte type = packet.ReadByte();

                Vector3 pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                if (p.Rank != Rank.ADMIN && p.Rank != Rank.MOD)
                {
                    if (p.PlayerTracking.IsTooFast(pos, p.Map.LandID))
                    {
                        p.NetworkActions.SendLocation();
                        p.PlayerTracking._lastVelocityCheckLocation = p.transform.position;
                        return;
                    }
                }

                if (p.Trading) {
                    TRADING.ResetTrade(p);
                    return;
                }

                float roty = packet.ReadFloat();
                p.rotation = roty;
                Vector3 velocity = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                bool force = packet.ReadBoolean();
                p.NetworkActions.SendMovement(pos, roty, velocity, type, force);
                p.transform.position = pos;

                if (Environment.TickCount - p.LastMovementViewportCheck > 1000) {
                    p.Viewport.UpdateSurroundingNpcs();
                    p.LastMovementViewportCheck = Environment.TickCount;
                }

            } else if (subType == 1) // jump
              {
                int jump = packet.ReadInt32();
                p.NetworkActions.SendJump(jump);
            }

            if (subType == 2) // roll
            {
                if (DateTime.Now.Ticks - p.PlayerTracking.LastRoll < 20000000) {
                    return;
                }
                p.PlayerTracking.LastRoll = DateTime.Now.Ticks;
                int action = packet.ReadInt32();
                p.NetworkActions.SendRoll(action);
            }

            if (subType == 3) // swim
            {
                int swim = packet.ReadInt32();
                bool trigger = packet.ReadBoolean();

                p.NetworkActions.SendSwim(swim, trigger);
            }

            if (subType == 5) // follow
            {
                uint uid = packet.ReadUInt32();
                Player target = Server.GetPlayerByUID(uid);
                if (target != null) {
                    if (Formula.InRadius(p, target, 15)) {
                        p.NetworkActions.SendFollow(target);
                    }
                }
            }
        }
    }
}
