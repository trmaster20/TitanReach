using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TitanReach_Server.Model;
using TRShared;

namespace TitanReach_Server.Plugins.AI
{

    public class NpcAI
    {
        protected Npc Npc;

        public NpcAI(Npc npc)
        {
            this.Npc = npc;
            InitStates();
            InitStateTransitions();
        }

        protected virtual void InitStates()
        {
            State spawningState = new State();
            spawningState.SetOnEntry(OnSpawnEntry);
            spawningState.SetOn(OnSpawn);
            spawningState.SetOnExit(OnSpawnExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.SPAWN, spawningState);

            State idlingState = new State();
            idlingState.SetOnEntry(OnIdleEntry);
            idlingState.SetOn(OnIdle);
            idlingState.SetOnExit(OnIdleExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.IDLE, idlingState);

            State deadState = new State();
            deadState.SetOnEntry(OnDeadEntry);
            deadState.SetOn(OnDead);
            deadState.SetOnExit(OnDeadExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.DEAD, deadState);

            State despawnedState = new State();
            despawnedState.SetOnEntry(OnDespawnEntry);
            despawnedState.SetOn(OnDespawn);
            despawnedState.SetOnExit(OnDespawnExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.DESPAWNED, despawnedState);

            State walkingState = new State();
            walkingState.SetOnEntry(OnWalkingEntry);
            walkingState.SetOn(OnWalking);
            walkingState.SetOnExit(OnWalkingExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.WALK, walkingState);

            State chasingState = new State();
            chasingState.SetOnEntry(OnChaseEntry);
            chasingState.SetOn(OnChase);
            chasingState.SetOnExit(OnChaseExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.CHASE, chasingState);

        }

        protected virtual void InitStateTransitions()
        {
            Npc.StateMachine.RegisterTransition((uint)NPCStates.SPAWN, (uint)NPCStates.IDLE, TimeCheck);

            uint[] toDeadStates = { (uint)NPCStates.IDLE, (uint)NPCStates.WALK, (uint)NPCStates.CHASE };
            Npc.StateMachine.RegisterMultipleTransition(toDeadStates, (uint)NPCStates.DEAD, () =>
            {
                return Npc.CurrentHealth <= 0;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.DEAD, (uint)NPCStates.DESPAWNED, TimeCheck);

            Npc.StateMachine.RegisterTransition((uint)NPCStates.DESPAWNED, (uint)NPCStates.SPAWN, () => {
                return Environment.TickCount - Npc.LastDeathTime > 15000;// Npc.SpawnDefinition.RespawnTime;
            });

            if (Npc.CanMove && !Npc.Definition.Friendly)
            {
                Npc.StateMachine.RegisterTransition((uint)NPCStates.IDLE, (uint)NPCStates.WALK, TimeCheck);

                Npc.StateMachine.RegisterTransition((uint)NPCStates.WALK, (uint)NPCStates.IDLE, TimeCheck);
            }
        }

        protected bool TimeCheck()
        {
            return Npc.CurrentStateTime >= Npc.StateTotalTime;
        }

        protected void SetState(NPCStates state, int maxStateTime = 0, int deviation = 0)
        {
            Npc.CurrentState = state;
            Npc.StateStartTime = Environment.TickCount;
            Npc.StateTotalTime = deviation == 0 ? maxStateTime : maxStateTime - (int)(TRShared.Data.Formula.rand.NextDouble() * deviation);
            Npc.SendAnimation((int)state);
        }

        // SPAWN states
        void OnSpawnEntry()
        {
            Npc.Alive = true;
            SetState(NPCStates.SPAWN, 200);
            //Npc.SendTargetLock(0);

            Npc.Respawn();
        }
        void OnSpawn()
        {

        }
        void OnSpawnExit()
        {

        }

        // IDLE states
        void OnIdleEntry()
        {
            if((Npc.Transform.position - Npc.StartTransform).Magnitude() > Npc.MaxChaseRange)
            {
                Npc.Transform.position = Npc.StartTransform;
                Npc.ResetHealth();
                Npc.Invulnerable = true;

                Timer t = new Timer();
                t.Interval = 5000;
                t.AutoReset = false;
                t.Elapsed += delegate { Npc.Invulnerable = false; };
                t.Start();

                foreach (Player p in Npc.AgroTargets.Keys){
                    p.Error("Npc Dragged Too Far");
                }
            }

            Npc.Alive = true;

            SetState(NPCStates.IDLE, 5000);
            Npc.SendTargetLock((int)TargetType.NONE);

            Npc.AgroTargets.Clear();
            Npc.AgroTarget = null;
            Npc.StateTrigger = false;

        }
        void OnIdle()
        {

        }
        void OnIdleExit()
        {

        }

        // DEAD states
        void OnDeadEntry()
        {
            Npc.Alive = false;

            Npc.LastDeathTime = Environment.TickCount;
            SetState(NPCStates.DEAD, 3000);
            Npc.SendTargetLock((int)TargetType.NONE);
            if (Npc.Killer != null)
            {
                //update kill listeners
                if (Npc.Killer.ACTION_OnNpcKill.ContainsKey(Npc.ID))
                {
                    Npc.Killer.ACTION_OnNpcKill[Npc.ID](Npc.Killer);
                }
                Npc.Killer.TriggerRaiseNPCKilledEvent(Npc.ID);
            }
        }

        void OnDead()
        {
            
        }
        void OnDeadExit()
        {

        }

        // DESPAWN Entry
        void OnDespawnEntry()
        {
            Npc.Alive = false;

            SetState(NPCStates.DESPAWNED, 3000);
            Npc.DeSpawn();

            if (Npc.NeedsCleanup) Npc = null;
        }
        void OnDespawn()
        {

        }
        void OnDespawnExit()
        {

        }

        // WALKING states
        void OnWalkingEntry()
        {
            Npc.SendTargetLock((int)TargetType.NONE);

            //float wanderDistance = Npc.SpawnDefinition.Radius / 2;
            //float angle = (float)TRShared.Data.Formula.rand.NextDouble() * 2 * MathF.PI;
            //float wanderX = MathF.Cos(angle) * wanderDistance + Npc.StartTransform.X;
            //float wanderZ = MathF.Sin(angle) * wanderDistance + Npc.StartTransform.Z;
            //Npc.StateTargetPosition = new Vector3(wanderX, DataManager.GetHeight((int)wanderX, (int)wanderZ, Npc.Map.Land), wanderZ);

            Npc.StateTargetPosition = Npc.PointInWander(true);
            Npc.StateStartPosition = Npc.Transform.position;
            float distanceToNewPosition = (Npc.StateTargetPosition - Npc.StateStartPosition).Magnitude();
            int totalStateTime = (int)((distanceToNewPosition / Npc.Definition.MoveSpeed) * 1000.0f);

            if (totalStateTime > 100)
            {
                Npc.SendTargetPosition(Npc.StateTargetPosition, totalStateTime);
                SetState(NPCStates.WALK, totalStateTime);
            }
            else
            {
                Npc.StateTargetPosition = Npc.StateStartPosition;
            }

        }
        void OnWalking()
        {
            //Debug.Log()

            //if ((Npc.StateStartTime - Npc.StateStartPosition).Magnitude() > 100) //condition never happens
            //{
            //Server.Log("On Walking")
                Npc.Transform.position = Formula.Lerp(Npc.StateStartPosition, Npc.StateTargetPosition, ((float)(Environment.TickCount - Npc.StateStartTime)) / ((float)(Npc.StateTotalTime)));
                Npc.SendExactPosition();
            //}
        }
        void OnWalkingExit()
        {
            float lerpValue = ((float)(Environment.TickCount - Npc.StateStartTime)) / ((float)(Npc.StateTotalTime));
            if (lerpValue > 1) lerpValue = 1;
            Npc.Transform.position = Formula.Lerp(Npc.StateStartPosition, Npc.StateTargetPosition, lerpValue);
            Npc.SendExactPosition();
        }


        // CHASE states
        void OnChaseEntry()
        {

            Npc.AgroTarget = Npc.AgroTargets.FirstOrDefault(x => x.Value == Npc.AgroTargets.Values.Max()).Key;
            SetState(NPCStates.CHASE);
            Npc.SendTargetLock((int)TargetType.PLAYER);
            //Npc.SendTargetPosition(Npc.Transform.position, true, Npc.AgroTarget.UID);

        }
        void OnChase()
        {
            Vector3 moveVector = Npc.AgroTarget.transform.position - Npc.Transform.position;
            Npc.DistnceToAgroTarget = moveVector.Magnitude();
            if (Npc.DistnceToAgroTarget < Npc.MeleeAttackRange) return;
            Npc.StateTotalTime = (int)((moveVector.Magnitude() / (Npc.Definition.MoveSpeed * 1.5)) * 1000.0f);
            Npc.Transform.position = Formula.Lerp(Npc.Transform.position, Npc.AgroTarget.transform.position, (200.0f) / ((float)(Npc.StateTotalTime)));
            Npc.SendTargetPosition(Npc.Transform.position, Npc.StateTotalTime);
            Npc.SendExactPosition();
        }
        void OnChaseExit()
        {
        }
    }
}
