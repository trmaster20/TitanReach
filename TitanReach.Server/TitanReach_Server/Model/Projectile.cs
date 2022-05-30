using System;
using System.Collections.Generic;

using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model
{
    public class Projectile
    {

        public static int CURRENT_PROJECTLES = 0;

        public int UID;
        public int ID;
        public int StartTime;
        public int LastTime;
        public bool Used = false;
        public List<Player> Targets;
        public int deltaTime;
        public Vector3 position;
        public Vector3 direction;
        public bool remove = false;
        public Npc SenderNpc;
        public Player SenderPlayer;

        public int AL = 0;
        public int AB = 0;

        public int PL = 0;
        public int PB = 0;
        public uint projID = 0;

        public ProjectileDefinition Definition
        {
            get
            {
                return DataManager.ProjectileDefinitions[ID];
            }
        }

        public Projectile(int id, Npc sender)
        {
            SenderNpc = sender;
            Prepare(id);
        }

        public Projectile(int id, Player sender, uint pID)
        {
            SenderPlayer = sender;
            Prepare(id);

            AL = SenderPlayer.GetAccuracyLevel(Definition.damageType);
            AB = SenderPlayer.GetAccuracyBonus(Definition.damageType);
            PL = SenderPlayer.GetPowerLevel(Definition.damageType);
            PB = SenderPlayer.GetPowerBonus(Definition.damageType);
            projID = pID;
        }

        public Projectile(int id)
        {
            Prepare(id);
        }

        public void Prepare(int id)
        {
            UID = CURRENT_PROJECTLES;
            CURRENT_PROJECTLES++;
            Targets = new List<Player>();
            ID = id;
            StartTime = Environment.TickCount;
            LastTime = StartTime;
            remove = false;
        }

        public void SetTargets(List<Player> targets)
        {
            Targets = targets;
        }

        public void Update()
        {
            int currentTime = Environment.TickCount;
            deltaTime = currentTime - LastTime;
            LastTime = currentTime;

            position += direction * Definition.speed * (deltaTime / 1000.0f);

            if (Targets != null)
            {
                if (Targets.Count > 0)
                {
                    CheckCollision();
                }
            }


            if (currentTime - StartTime > Definition.time * 1000)
            {
                remove = true;
            }
        }

        public void CheckCollision()
        {
            foreach (Player p in Targets)
            {

                if ((p.transform.position - position).Magnitude() < 5)
                { //def.radius3* for safety factor to make sure the detailed check gets used
                  //  Server.Log("Basic Sphere Check Passed");

                    //Perform capuse check
                    Vector3 ABase = p.transform.position;
                    Vector3 ATip = ABase + new Vector3(0, 1.9f, 0);
                    float ARadius = 0.25f;
                    Vector3 BBase = position;
                    Vector3 BTip = position - direction * Definition.speed * (deltaTime / 1000.0f);
                    float BRadius = Definition.radius;// Definition.radius;

                    if (Formula.CapsuleCollision(ATip, ABase, ARadius, BTip, BBase, BRadius))
                    {
                        // Server.Log("Capsule Check Passed");


                        if (DataManager.ProjectileDefinitions[ID].TriggerBuffOnHit != -1)
                        {
                            if (SenderNpc != null)
                            {
                                if (SenderNpc.ID == 8)
                                {
                                    //p.ApplyBuff(Buff.Freeze(1, -1, 7000));
                                }
                            }

                        }
                        remove = true;
                        DamageCalculator.NPCAttackPlayer(SenderNpc, p, DamageType.RANGED); //TODO NOT JUST RANGED
                        return;
                    }
                    else
                    {
                        // Server.Log("Capsule Check Failed");

                    }
                }
            }
        }

        public void Destroy()
        {
            CURRENT_PROJECTLES--;
        }

    }
}
