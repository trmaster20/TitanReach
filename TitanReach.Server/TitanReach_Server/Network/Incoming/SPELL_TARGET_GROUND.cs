using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Skills;
using TRShared;

namespace TitanReach_Server.Network.Incoming
{
    class SPELL_TARGET_GROUND : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.SPELL_TARGET_GROUND;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subtype = packet.ReadByte();
            if (subtype == 0) {
                int type = (int)packet.ReadUInt32();
                Vector3 pos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                p.Map.Players.ForEach(pl => pl?.NetworkActions.SpellTargetGround(pos, type));

                Server.Instance.LoopedDelay(7, 1000, (timer, arg) => {
                    p?.Viewport.NpcsInView.ForEach(n => {
                        if (n != null && Formula.InRadiusXZ(pos, n.Transform.position, 15))
                            n.Damage(TRShared.Data.Formula.rand.Next(1, 4), p, TRShared.Data.Enums.DamageType.MAGIC);
                    });
                });
            } else if (subtype == 1) {

                int spellID = packet.ReadByte();
                if (p.Busy) {
                    p.NetworkActions.SendMessage("You cant cast this spell while busy");
                    return;
                }

                Arcana.CastSpell(p, spellID);
            }
        }
    }
}
