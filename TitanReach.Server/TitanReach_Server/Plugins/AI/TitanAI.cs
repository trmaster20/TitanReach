using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Plugins.AI
{
    class TitanAI : NpcAI
    {
        public TitanAI(Npc npc) : base(npc)
        {

        }

        protected override void InitStates()
        {
            base.InitStates();

            State roaringState = new State();
            roaringState.SetOnEntry(OnRoarEntry);
            roaringState.SetOn(OnRoar);
            roaringState.SetOnExit(OnRoarExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.ROAR, roaringState);

            State melleAttackState = new State();
            melleAttackState.SetOnEntry(OnMeleeEntry);
            melleAttackState.SetOn(OnMelee);
            melleAttackState.SetOnExit(OnMeleeExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.ATTACK, melleAttackState);


            State specialAttackState = new State();
            specialAttackState.SetOnEntry(OnSpecialEntry);
            specialAttackState.SetOn(OnSpecial);
            specialAttackState.SetOnExit(OnSpecialExit);
            Npc.StateMachine.RegisterState((uint)NPCStates.SPECIAL, specialAttackState);

        }

        protected override void InitStateTransitions()
        {
            base.InitStateTransitions();

            uint[] toRoarStates = { (uint)NPCStates.IDLE, (uint)NPCStates.WALK };
            Npc.StateMachine.RegisterMultipleTransition(toRoarStates, (uint)NPCStates.ROAR, () =>
            {
                if (Npc.Definition.Aggressive) Npc.PopulateAgroTargets();
                return Npc.AgroTargets.Count > 0;
            });

            uint[] toRoarStates2 = { (uint)NPCStates.ATTACK, (uint)NPCStates.CHASE };
            Npc.StateMachine.RegisterMultipleTransition(toRoarStates2, (uint)NPCStates.ROAR, () =>
            {
                if (Npc.StateTrigger == false && Npc.HealthPercentage() < 0.5)
                {
                    Npc.StateTrigger = true;
                    return true;
                }
                return false;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.ROAR, (uint)NPCStates.CHASE, TimeCheck);

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.ATTACK, () =>
            {
                return Npc.DistnceToAgroTarget < Npc.MeleeAttackRange;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.IDLE, () =>
            {
                return Npc.DistnceToAgroTarget > Npc.GetMaxChaseRange() && Npc.CurrentStateTime > 3000;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.CHASE, TimeCheck);

            uint[] toDeadStates = { (uint)NPCStates.WALK, (uint)NPCStates.CHASE, (uint)NPCStates.ATTACK };
            Npc.StateMachine.RegisterMultipleTransition(toDeadStates, (uint)NPCStates.DEAD, () =>
            {
                return Npc.CurrentHealth <= 0;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.SPECIAL, () =>
            {
                if (!Npc.StateTrigger) return false;
                return Npc.DistnceToAgroTarget < Npc.ArcherAttackRange && Npc.CurrentStateTime > 1500;
            });

            Npc.StateMachine.RegisterTransition((uint)NPCStates.SPECIAL, (uint)NPCStates.CHASE, TimeCheck);

        }

        // ROARING states
        void OnRoarEntry()
        {
            SetState(NPCStates.ROAR, 3500);
        }
        void OnRoar()
        {

        }
        void OnRoarExit()
        {

        }

        // MELEE States
        void OnMeleeEntry()
        {
            SetState(NPCStates.ATTACK, 2500);
            Server.Instance.Delay(1000, (timer, arg) =>
            {
                if (Npc.AgroTarget != null)
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

        // SPECIAL States
        void OnSpecialEntry()
        {
            SetState(NPCStates.SPECIAL, 3000);

            if (Npc.AgroTarget != null)
            {
                if (!Npc.AgroTarget.Dead)
                {

                    Server.Instance.Delay(1500, (timer, arg) =>
                    {
                        Vector3 TargetPos = Npc.AgroTarget.transform.position;

                        Server.Log(TargetPos);
                        Npc.SendSpecial(TargetPos, 1);

                        Server.Instance.Delay(600, (timer, arg) =>
                        {
                            foreach (Player p in Npc.AgroTargets.Keys)
                            {
                                if (p != null) //TODO - remove all these stupid null checks 
                                {
                                    if (!p.Dead)
                                    {
                                        if ((p.transform.position - TargetPos).Magnitude() < 5) //TODO - remove 5
                                        {
                                            DamageCalculator.NPCAttackPlayer(Npc, p, DamageType.RANGED); //TODO NOT JUST RANGED

                                        }
                                    }
                                }
                            }
                        });

                    });



                }
            }


        }
        void OnSpecial()
        {
        }
        void OnSpecialExit()
        {

        }
    }
}
