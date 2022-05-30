using System;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model
{
    public class Buff
    {

        public int BuffID;
        public int Duration;
        public int StartTime;
        public float[] ArgArray;
        public int LastTick = Environment.TickCount;

        public Buff(int id, int duration, params float[] arg)
        {
            ArgArray = arg;
            BuffID = id;
            Duration = duration;

            StartTime = Environment.TickCount;
        }


        public Action<Player> OnTick;

        public BuffDef GetDef()
        {
            return DataManager.BuffDefinitions[BuffID];
        }

        public static bool BuffCastCheck(Player p)
        {
            if (p.Dead)
                return false;


            return true;
        }

        public static Buff Heal(int id, int health, int duration)
        {
            Buff b = new Buff(id, duration);
            b.ArgArray = new float[] { health, duration };
            b.OnTick = (player) =>
            {
                if (!BuffCastCheck(player))
                    return;
                float heal = ((float)health / (duration / 1000)) * (b.GetDef().TickRate / 1000);
                player.Heal((int)heal);
            };

            return b;

        }

        public static Buff Damage(int id, int damage, int duration)
        {
            Buff b = new Buff(id, duration);
            b.ArgArray = new float[] { damage, duration };
            b.OnTick = (player) =>
            {
                if (player.Dead)
                    return;
                float dmg = ((float)damage / (duration / 1000)) * (b.GetDef().TickRate / 1000);
                player.Damage((int)dmg, null, DamageType.MAGIC);
            };

            return b;

        }

        public static Buff Freeze(int id, int damage, int duration)
        {
            Buff b = new Buff(id, duration);
            b.ArgArray = new float[] { damage, duration };
            if (b.GetDef().TickRate != -1 && damage > 0)
            {
                b.OnTick = (player) =>
                {
                    if (player.Dead)
                        return;
                    float dmg = ((float)damage / (duration / 1000)) * (b.GetDef().TickRate / 1000);
                    player.Damage((int)dmg, null, DamageType.MAGIC);
                };
            }



            return b;

        }
    }
}
