using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Plugins.AI
{
    class MeleeAI : NpcAI
    {
        public MeleeAI(Npc npc) : base(npc)
        {

        }

        protected override void InitStates()
        {
            base.InitStates();

            State melleAttackState = new State();
            melleAttackState.SetOnEntry(OnMeleeEntry);
            melleAttackState.SetOn(OnMelee);
            melleAttackState.SetOnExit(OnMeleeExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.ATTACK, melleAttackState);

        }

        protected override void InitStateTransitions()
        {
            base.InitStateTransitions();

            uint[] toChaseStates = { (uint)NPCStates.IDLE, (uint)NPCStates.WALK };
            Npc.StateMachine.RegisterMultipleTransition(toChaseStates, (uint)NPCStates.CHASE, () =>
            {
                if (Npc.Definition.Aggressive) Npc.PopulateAgroTargets();
                if ((Npc.Transform.position - Npc.StartTransform).Magnitude() > Npc.MaxChaseRange - 5) return false;
                return Npc.AgroTargets.Count > 0;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.ATTACK, () =>
            {
                if (Npc.AgroTarget == null || !Npc.Alive) return false;
                if (Npc.AgroTarget.Dead) return false;
                return Npc.DistnceToAgroTarget < Npc.Definition.AttackRadius;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.IDLE, () =>
            {
                if (Npc.AgroTarget == null || Npc.AgroTarget.Dead == true || (Npc.AgroTarget != null && Npc.AgroTarget.Map != Npc.Map)) return true;
                if (Npc.DistnceToAgroTarget > Npc.GetMaxChaseRange() && Npc.CurrentStateTime > 3000) return true;
                if ((Npc.Transform.position - Npc.StartTransform).Magnitude() > Npc.MaxChaseRange) return true;
                return false;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.CHASE, TimeCheck);

            Npc.StateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.DEAD, () =>
            {
                return Npc.CurrentHealth <= 0;
            });
        }

        // Melee States
        void OnMeleeEntry()
        {
            SetState(NPCStates.ATTACK, 1000);
            Npc.SendTargetLock((int)TargetType.ANGLE);

            Server.Instance.Delay(350, (timer, arg) =>
            {
                if (Npc.AgroTarget != null && Npc.Alive)
                {
                    if (!Npc.AgroTarget.Dead)
                    {
                        if ((Npc.AgroTarget.transform.position - Npc.Transform.position).Magnitude() < Npc.MeleeAttackRange)
                        {
                            DamageCalculator.NPCAttackPlayer(Npc, Npc.AgroTarget, DamageType.MELEE);
                        }
                    }
                }
            });
        }
        void OnMelee()
        {

        }
        void OnMeleeExit()
        {

        }
    }
}
