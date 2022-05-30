using System.Collections.Generic;
using TitanReach_Server.Model;

namespace TitanReach_Server.Plugins.AI
{
    class MageAI : NpcAI
    {
        public MageAI(Npc npc) : base(npc)
        {

        }

        protected override void InitStates()
        {
            base.InitStates();

            State castState = new State();
            castState.SetOnEntry(OnCastEntry);
            castState.SetOn(OnCast);
            castState.SetOnExit(OnCastExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.CAST_SPELL, castState);
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

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.CAST_SPELL, () =>
            {
                if (Npc.AgroTarget == null || !Npc.Alive) return false;
                if (Npc.AgroTarget.Dead) return false;
                return Npc.DistnceToAgroTarget < Npc.ArcherAttackRange;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CAST_SPELL, (uint)NPCStates.CAST_SPELL, () =>
            {
                if (Npc.AgroTarget == null || !Npc.Alive) return false;
                if (Npc.AgroTarget.Dead) return false;
                Npc.DistnceToAgroTarget = (Npc.AgroTarget.transform.position - Npc.Transform.position).Magnitude();
                return (Npc.CurrentStateTime >= Npc.StateTotalTime && Npc.DistnceToAgroTarget < Npc.ArcherAttackRange);
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CAST_SPELL, (uint)NPCStates.CHASE, () =>
            {
                Npc.DistnceToAgroTarget = (Npc.AgroTarget.transform.position - Npc.Transform.position).Magnitude();
                return (Npc.CurrentStateTime >= Npc.StateTotalTime && Npc.DistnceToAgroTarget > Npc.ArcherAttackRange) && !Npc.AgroTarget.Dead;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.IDLE, () =>
            {
                return Npc.DistnceToAgroTarget > Npc.GetMaxChaseRange() && Npc.CurrentStateTime > 3000 && Npc.AgroTarget != null && !Npc.AgroTarget.Dead;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CAST_SPELL, (uint)NPCStates.DEAD, () =>
            {
                return Npc.CurrentHealth <= 0;
            });

        }

        //LOAD_ARROW states
        void OnCastEntry()
        {
            if (Npc.AgroTarget != null && Npc.AgroTarget.Dead)
                return;

            SetState(NPCStates.CAST_SPELL, 1500);
            Npc.SendTargetLock((int)TargetType.PLAYER);

            Server.Instance.Delay(350, (timer, arg) =>
            {
                Vector3 startPos = Npc.Transform.position + new Vector3(0, 1, 0);
                Vector3 direction = (Npc.AgroTarget.transform.position + new Vector3(0, 1, 0) - startPos);
                direction.Normalize();
                //direction = direction * (arrowSpeed / 20.0f);
                Projectile arrow = new Projectile(3, Npc);
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
            });
        }
        void OnCast()
        {

        }
        void OnCastExit()
        {

        }
    }
}
