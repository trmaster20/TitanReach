using System;
using System.Collections.Generic;
using System.Linq;
using TitanReach_Server.Plugins.AI;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model
{
    public enum NPCStates
    {
        //SPAWN,
        //IDLE,
        //DEAD,
        //DESPAWNED,
        //WALK,
        //CHASE,
        //ATTACK_MELEE,
        //ROAR

        IDLE,
        WALK,
        RUN,
        ROLL,
        JUMP,
        STRAFE,
        CHASE,
        ATTACK,
        DEAD,
        DESPAWNED,
        INTERACTING,
        LOAD_ARROW,
        HOLD_ARROW,
        RELEASE_ARROW,
        CAST_SPELL,
        SPAWN,
        ATTACK_MELEE,
        ROAR,
        SPECIAL,
        SWIMMING,
        FOLLOWING_PLAYER,
    }

    public enum AITypes
    {
        FriendlyHuman,
        MeleeAI,
        ArcherAI,
        MageAI,
        TitanAI,
    }

    public enum TargetType
    {
        NONE = 4,
        PLAYER = 5,
        ANGLE = 6,
    }

    public class Npc
    {

        public enum IconType
        {
            SHOP, EXIT, QUESTION, ACCEPT
        }

        public class DialogOption
        {
            public DialogOption(Npc.IconType icon, string text)
            {
                this.Icon = icon;
                this.Text = text;
            }
            public Npc.IconType Icon;
            public string Text;
        }

        public int LastInteractAction = 0;
        private bool Busy {
            get {
                return busyCounter > 0;
            }
        }

        private int busyCounter = 0;

        public void SetBusy() {
            busyCounter++;
        }

        public void SetNotBusy() {
            busyCounter--;
        }

        public int MapID = 0;
        public Map Map => Server.Instance.Maps[MapID];

        static int NPC_COUNTER = 2000;
        public int UID;
        public int ID;
        public int NpcSpawnID;
        public int LastDeathTime = Environment.TickCount;
        public bool NeedsCleanup = false;
        public bool Invulnerable = false;

        public Player Killer = null;

        public bool Alive = true;

        public bool CanMove {
            get
            {
                return !SpawnDefinition.ForceStationary || (SpawnDefinition.RangeWidth == 0 && SpawnDefinition.RangeHeight == 0);
            }
        }


        public NpcDef Definition
        {
            get
            {
                return DataManager.NpcDefinitions[ID];
            }
        }
        public NpcSpawnDef SpawnDefinition
        {
            get
            {
                return Map.Land.NpcSpawnDefinitions[NpcSpawnID];
            }
        }
        public Vector3 StartTransform;

        public int CurrentHealth;
        public bool Dead = false;
        public bool DeSpawned = false;

        public Transform Transform;
        public double Scale = 1;

        public float ChaseRadius;         //def? spwaner?
        public float AgroRadius = 5.0f;

        public Dictionary<Player, int> AgroTargets;
        public Player AgroTarget;


        public float MaxChaseRange = 30;
        public float DeAgroDistance = 25.0f;
        public float MinAgroTime = 5000;


        public float ArcherAttackRange = 15;
        public float ArcherAttackRangeExit = 20;
        public float MeleeAttackRange = 3;
        public float DistnceToAgroTarget;



        public Vector3 StateTargetPosition = null;
        public Vector3 StateStartPosition = null;

        public NpcAI NpcAI;
        public StateMachine StateMachine;
        public NPCStates CurrentState = NPCStates.IDLE;
        public int CurrentStateTime = 0;
        public int StateStartTime = 0;
        public int StateTotalTime = 0;

        public bool StateTrigger = false;
        public System.Random rand;

        public Npc(int id, int spwanId, int MapID)
        {
            this.MapID = MapID;
            rand = new System.Random();
            ID = id;
            UID = NPC_COUNTER;
            NpcSpawnID = spwanId;
            
            NPC_COUNTER++;
            Transform = new Transform(Vector3.Zero(), Vector3.Zero(), Vector3.Zero());

            StateMachine = new StateMachine();
            AgroTargets = new Dictionary<Player, int>();
            StateStartTime = Environment.TickCount;

            CurrentHealth = Definition.MaxHealth;
            
            Transform.position = new Vector3(SpawnDefinition.Position);
            StartTransform = Transform.position;
            //npc.Scale = GetRandomDouble(npc.Definition.AverageScale - npc.Definition.ScaleDeviation, npc.Definition.AverageScale + npc.Definition.ScaleDeviation);


            switch (Definition.AiType)
            {
                case (int)AITypes.FriendlyHuman:
                    NpcAI = new NpcAI(this);
                    break;
                case (int)AITypes.MeleeAI:
                    NpcAI = new MeleeAI(this);
                    break;
                case (int)AITypes.ArcherAI:
                    NpcAI = new ArcherAI(this);
                    break;
                case (int)AITypes.MageAI:
                    NpcAI = new MageAI(this);
                    break;
                case (int)AITypes.TitanAI:
                    NpcAI = new TitanAI(this);
                    break;
                default:
                    //Server.Log("invalid npc AI ID");
                    NpcAI = new NpcAI(this);
                    break;
            }
        }

        public void Update()
        {
            CurrentStateTime = Environment.TickCount - StateStartTime;
            StateMachine.Run();
        }

        public float HealthPercentage()
        {
            return ((float)CurrentHealth / (float)Definition.MaxHealth);
        }

        public float GetMaxChaseRange()
        {
            return System.MathF.Max(SpawnDefinition.RangeWidth, SpawnDefinition.RangeHeight) * 2;
        }

        public int Damage(int damage, Player attacker, DamageType source)
        {

            if (Dead || CurrentHealth <= 0)
            {
                damage = 0;
            }
            else
            {
                if (damage > CurrentHealth) damage = CurrentHealth;
                CurrentHealth = CurrentHealth - damage;
            }

            if (!AgroTargets.ContainsKey(attacker)) {
                AgroTargets.Add(attacker, damage);
            } else {
                AgroTargets[attacker] += damage;
            }


            Killer = AgroTargets.FirstOrDefault(x => x.Value == AgroTargets.Values.Max()).Key;

            // StateMachine.Run(); // mabye
            //SendTargetPosition(new Vector3(0, 0, 0), true, agroTarget.UID, -1);
            lock (Map.Players)
            {
                foreach (Player pla in Map.Players)
                {
                   if(pla == attacker)
                       pla.NetworkActions.SendHitSplat(null, this, damage, (int)source);
                    

                    pla.NetworkActions.UpdateNpc(this);
                }
            }

            return damage;

        }

        public void ResetHealth()
        {
            CurrentHealth = Definition.MaxHealth;
            
            lock (Map.Players)
            {
                foreach (Player pla in Map.Players)
                {
                    pla.NetworkActions.UpdateNpc(this);
                }
            }
        }

        public void PopulateAgroTargets()
        {
            lock (Map.Players)
            {
                for (int i = 0; i < Map.Players.Count; i++)
                {
                    try
                    {
                        Player pl = Map.Players[i];
                        if (pl != null)
                        {
                            if (pl.Viewport.NpcsInView.Contains(this))
                            {
                                float ditanceToPlayer = (pl.transform.position - Transform.position).Magnitude();

                                if (ditanceToPlayer <= AgroRadius && !pl.Dead)
                                {
                                    if (!AgroTargets.ContainsKey(pl)) {
                                        AgroTargets.Add(pl, 0);
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Server.Error(e.Message + " - " + e.StackTrace);
                    }
                }


            }
        }

        public void SendAnimation(int state)
        {
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        pl.NetworkActions.SendNpcAnimation(state, this);

                    }
                }
            }
        }

        public void SendSpecial(Vector3 position, int type)
        {
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        pl.NetworkActions.SendSpecialAttack(position, type);

                    }
                }
            }
        }

        public Vector3 PointInWander(bool queryHeigtMap = false)
        {
            Vector3 pointInWander = Vector3.Zero();
            float RangeRotation = -SpawnDefinition.RangeRotation * ( MathF.PI / 180.0f); //negative here turns positive rotationg
            if (SpawnDefinition.WanderType == 0) // oval
            {
                float phi = MathF.Sqrt((float)rand.NextDouble());
                float theta = (float)rand.NextDouble() * 2 * MathF.PI;
                float unscaledX = MathF.Cos(theta) * phi;
                float unscaledY = MathF.Sin(theta) * phi;
                float x = unscaledX * SpawnDefinition.RangeWidth;
                float z = unscaledY * SpawnDefinition.RangeHeight;

                float xtemp = x;
                float ztemp = z;
                if (SpawnDefinition.RangeRotation != 0)
                {
                    
                    x = xtemp * MathF.Cos(RangeRotation) - ztemp * MathF.Sin(RangeRotation);
                    z = xtemp * MathF.Sin(RangeRotation) + ztemp * MathF.Cos(RangeRotation);
                }
 
                x += SpawnDefinition.Position.X; 
                z += SpawnDefinition.Position.Z;

                //if(SpawnDefinition.nameRef == "0004_Cow (1)")  //that was one hell of a desk check //client rangerotation was clockwise smh
                //{
                //    Server.Log("------------------------------------");
                //    Server.Log("SpawnDefinition.RangeWidth: " + SpawnDefinition.RangeWidth);
                //    Server.Log("SpawnDefinition.RangeHeight: " + SpawnDefinition.RangeHeight);
                //    Server.Log("SpawnDefinition.RangeRotation: " + SpawnDefinition.RangeRotation);
               
                //    Server.Log("phi: " + phi);
                //    Server.Log("theta: " + theta);
                //    Server.Log("unscaledX: " + unscaledX);
                //    Server.Log("unscaledY: " + unscaledY);
                //    Server.Log("xtemp: " + xtemp);
                //    Server.Log("ztemp: " + ztemp);
                //    Server.Log("x: " + (x - SpawnDefinition.Position.X));
                //    Server.Log("z: " + (z - SpawnDefinition.Position.Z));
                //    Server.Log("SpawnDefinition.Position.X: " + (SpawnDefinition.Position.X));
                //    Server.Log("SpawnDefinition.Position.Z: " + (SpawnDefinition.Position.Z));
                //    Server.Log("x " + (x));
                //    Server.Log("z " + (z));
                //    Server.Log("------------------------------------");

                //}

                if (queryHeigtMap)  pointInWander = new Vector3(x, DataManager.GetHeight((int)x, (int)z, Map.Land), z);
                else pointInWander = new Vector3(x, SpawnDefinition.Position.Y, z);
                

            }
            else if (SpawnDefinition.WanderType == 1) //rectangle
            {
                
                float x = ((float)rand.NextDouble() * 2 - 1) * SpawnDefinition.RangeWidth;
                float z = ((float)rand.NextDouble() * 2 - 1) * SpawnDefinition.RangeHeight;
                if (SpawnDefinition.RangeRotation != 0)
                {
                    float xtemp = x;
                    float ztemp = z;
                    x = xtemp * MathF.Cos(RangeRotation) - ztemp * MathF.Sin(RangeRotation);
                    z = xtemp * MathF.Sin(RangeRotation) + ztemp * MathF.Cos(RangeRotation);
                }
                x += SpawnDefinition.Position.X;
                z += SpawnDefinition.Position.Z;
                if (queryHeigtMap) pointInWander = new Vector3(x, DataManager.GetHeight((int)x, (int)z, Map.Land), z);
                else pointInWander = new Vector3(x, SpawnDefinition.Position.Y, z);

            }
            else if (SpawnDefinition.WanderType == 2)// Waypoint
            {
                Server.Log("Error - Waypoints not in yet");
            }
            else
            {
                Server.Log("This SpawnDef has invalid wander type");
            }
            return pointInWander;
        }

        public void Respawn()
        {
            Killer = null;
            CurrentHealth = Definition.MaxHealth;
            Dead = false;
            DeSpawned = false;
            busyCounter = 0;
            Transform.position = StartTransform;
            if (SpawnDefinition.SpawnInCenter)
            {
                Transform.position = new Vector3(SpawnDefinition.Position);
            }
            else
            {
                Transform.position = PointInWander();
               
            }
            //float radiusRand = (float)rand.NextDouble() * SpawnDefinition.Radius;
            //float angleRand = (float)rand.NextDouble() * 1000;
            //Transform.position += new Vector3(radiusRand * MathF.Cos(angleRand),0.6f, radiusRand * MathF.Sin(angleRand));
            //Transform.position = new Vector3(Transform.position.X, DataManager.GetHeight((int)Transform.position.X, (int)Transform.position.Z, Map.Land), Transform.position.Z);
            lock (Map.Players)
            {
                foreach (Player pla in Map.Players)
                {
                    if (!pla.Viewport.NpcsInView.Contains(this))
                        pla.Viewport.NpcsInView.Add(this);
                    pla.NetworkActions.SyncNpc(this);

                }
            }
        }

        public void DeSpawn()
        {
            DropDef dropTable = DataManager.DropTables[Definition.DropTableID];
            double pickValue = TRShared.Data.Formula.rand.NextDouble() * dropTable.TotalWeight;

            foreach (ItemDrop drop in dropTable.GuaranteedDrops)
            {
                
                GroundItem droppedItem = new GroundItem(drop.ItemID, MapID);

                if (drop.AmountMax == 0)
                {
                    droppedItem.Item.Amount = drop.Amount;
                }
                else
                {
                    droppedItem.Item.Amount = TRShared.Data.Formula.rand.Next(drop.AmountMin, drop.AmountMax + 1);
                }

                droppedItem.transform = new Transform(new Vector3(Transform.position.X, DataManager.GetHeight((int)Transform.position.X, (int)Transform.position.Z, Map.Land), Transform.position.Z), null, null);
                
                if (droppedItem.Item.Definition.MaxStackSize > 1 && droppedItem.Item.Amount <= 0)
                {
                    Server.Error("Problem with Item Drop npc.cs 1");
                    continue;
                }

                Map.AddItemToGround(droppedItem, Killer);
            }
            foreach (ItemDrop drop in dropTable.Drops)
            {
                if (drop.Weight >= pickValue)
                {
                    GroundItem droppedItem = new GroundItem(drop.ItemID, MapID);
                    droppedItem.Item.Amount = drop.Amount;
                    droppedItem.transform = new Transform(new Vector3(Transform.position.X, DataManager.GetHeight((int)Transform.position.X, (int)Transform.position.Z, Map.Land), Transform.position.Z), null, null);
                    if (droppedItem.Item.Definition.MaxStackSize > 1 && droppedItem.Item.Amount <= 0)
                    {
                        Server.Error("Problem with Item Drop npc.cs 1");
                        continue;
                    }

                    Map.AddItemToGround(droppedItem, Killer);
                    break;
                }
            }

            Dead = true;
            DeSpawned = true;
            lock (Map.Players)
            {
                foreach (Player pla in Map.Players)
                {
                    pla.NetworkActions.UpdateNpc(this);
                }
            }

            if (SpawnDefinition.RespawnTime == -1) {
                Map.Npcs.Remove(this);
                StateMachine = null;
                NeedsCleanup = true;
            }
        }

        public void SendExactPosition()
        {
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        pl.NetworkActions.SendNpcPosition(Transform.position, this);                       
                    }
                }
            }
        }

        public void SendDeAgro()
        {
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        pl.NetworkActions.SendNPCDeAgro(this);
                    }
                }
            }
        }

        public void SendTargetPosition(Vector3 position, int p_totalStateTime = 0)
        {
            //could be optimised to make sure the target moved before sending more packets
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        pl.NetworkActions.SendNpcTarget(position, p_totalStateTime, this);
                    }
                }
            }
        }

        uint lastAgroTargetID = 10000; //has to be uint and not sure if 0 is used // TODO fix 
        float angle = 0;
        public void SendTargetLock(int targetType = 0)
        {
            if (targetType == (int)TargetType.NONE)  //dont send repeat tagets
            {
                if (lastAgroTargetID == 10000) return;
                lastAgroTargetID = 10000;
            }
            else if (targetType == (int)TargetType.PLAYER)
            {
                if (AgroTarget.UID == lastAgroTargetID) return;
                lastAgroTargetID = AgroTarget.UID;
            }
            else if (targetType == (int)TargetType.ANGLE)
            {
                lastAgroTargetID = 10000;
                Vector3 npcPos = Transform.position;
                Vector3 targetPos = AgroTarget.transform.position;
                Vector3 direction = npcPos - targetPos;
                angle = MathF.Atan2(direction.Z, direction.X);
            }
            lock (Map.Players)
            {
                foreach (Player pl in Map.Players)
                {
                    if (pl.Viewport.NpcsInView.Contains(this))
                    {
                        if (targetType == (int)TargetType.NONE)
                        {
                            pl.NetworkActions.SendNpcTargetLock(targetType, UID);
                        }
                        else if (targetType == (int)TargetType.PLAYER)
                        {
                            pl.NetworkActions.SendNpcTargetLock(targetType, UID, AgroTarget.UID);
                        }
                        else if (targetType == (int)TargetType.ANGLE)
                        {
                            pl.NetworkActions.SendNpcTargetLock(targetType, UID, angle: angle);
                        }
                    }
                }
            }
        }
    }
}
