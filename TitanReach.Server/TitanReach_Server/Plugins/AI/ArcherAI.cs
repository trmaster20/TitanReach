using System.Collections.Generic;
using TitanReach_Server.Model;

namespace TitanReach_Server.Plugins.AI
{
    class ArcherAI : NpcAI
    {
        public ArcherAI(Npc npc) : base(npc)
        {

        }

        protected override void InitStates()
        {
            base.InitStates();

            State arrowLoadState = new State();
            arrowLoadState.SetOnEntry(OnArrowLoadEntry);
            arrowLoadState.SetOn(OnArrowLoad);
            arrowLoadState.SetOnExit(OnArrowLoadExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.LOAD_ARROW, arrowLoadState);

            State arrowReleaseState = new State();
            arrowReleaseState.SetOnEntry(OnArrowReleaseEntry);
            arrowReleaseState.SetOn(OnArrowRelease);
            arrowReleaseState.SetOnExit(OnArrowReleaseExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.RELEASE_ARROW, arrowReleaseState);

        }

        protected override void InitStateTransitions()
        {
            base.InitStateTransitions();

            uint[] toChaseStates = { (uint)NPCStates.IDLE, (uint)NPCStates.WALK };
            Npc.StateMachine.RegisterMultipleTransition(toChaseStates, (uint)NPCStates.CHASE, () =>
            {
                if (Npc.Definition.Aggressive) Npc.PopulateAgroTargets();
                return Npc.AgroTargets.Count > 0;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.LOAD_ARROW, () =>
            {
                if (Npc.AgroTarget == null || !Npc.Alive || (Npc.AgroTarget != null && Npc.AgroTarget.Map != Npc.Map)) return false;
                if (Npc.AgroTarget.Dead) return false;
                return Npc.DistnceToAgroTarget < Npc.ArcherAttackRange;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.LOAD_ARROW, (uint)NPCStates.RELEASE_ARROW, TimeCheck);

            Npc.StateMachine.RegisterTransition((uint)NPCStates.RELEASE_ARROW, (uint)NPCStates.LOAD_ARROW, () =>
            {
                if (Npc.AgroTarget == null || !Npc.Alive || (Npc.AgroTarget != null && Npc.AgroTarget.Map != Npc.Map)) return false;
                if (Npc.AgroTarget.Dead) return false;
                Npc.DistnceToAgroTarget = (Npc.AgroTarget.transform.position - Npc.Transform.position).Magnitude();
                return (Npc.CurrentStateTime >= Npc.StateTotalTime && Npc.DistnceToAgroTarget < Npc.ArcherAttackRange);
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.RELEASE_ARROW, (uint)NPCStates.CHASE, () =>
            {
                Npc.DistnceToAgroTarget = (Npc.AgroTarget.transform.position - Npc.Transform.position).Magnitude();
                return (Npc.CurrentStateTime >= Npc.StateTotalTime && Npc.DistnceToAgroTarget > Npc.ArcherAttackRange);
            });

        
            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.IDLE, () =>
            {
                return Npc.DistnceToAgroTarget > Npc.GetMaxChaseRange() && Npc.CurrentStateTime > 3000;
            });

            uint[] toDeadStates = { (uint)NPCStates.LOAD_ARROW, (uint)NPCStates.RELEASE_ARROW };
            Npc.StateMachine.RegisterMultipleTransition(toDeadStates, (uint)NPCStates.DEAD, () =>
            {
                return Npc.CurrentHealth <= 0;
            });
        }

        //LOAD_ARROW states
        void OnArrowLoadEntry()
        {
            SetState(NPCStates.LOAD_ARROW, 1500);
            Npc.SendTargetLock((int)TargetType.PLAYER);

        }
        void OnArrowLoad()
        {

        }
        void OnArrowLoadExit()
        {

        }

        //RELEASE_ARROW states
        void OnArrowReleaseEntry()
        {
            SetState(NPCStates.RELEASE_ARROW, 1000);
            Vector3 startPos = Npc.Transform.position + new Vector3(0, 1, 0);
            Vector3 direction = (Npc.AgroTarget.transform.position + new Vector3(0, 1, 0) - startPos);
            direction.Normalize();
            //direction = direction * (arrowSpeed / 20.0f);
            Projectile arrow = new Projectile(6, Npc);
            arrow.direction = direction;
            arrow.position = startPos - new Vector3(0, 1, 0);
            arrow.Targets = new List<Player>(Npc.AgroTargets.Keys);
            Npc.Map.Projectiles.Add(arrow);
            lock (Npc.Map.Players)
            {
                foreach (Player pl in Npc.Map.Players)
                {
                    pl.NetworkActions.SendNPCProjectile(arrow.Definition, direction, Npc);
                }
            }

            Npc.SendTargetLock((int)TargetType.ANGLE);

        }
        void OnArrowRelease()
        {

        }
        void OnArrowReleaseExit()
        {

        }
    }
}
