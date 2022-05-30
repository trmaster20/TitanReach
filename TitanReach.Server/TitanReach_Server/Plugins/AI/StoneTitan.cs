using System;
using TitanReach_Server.Model;
using TitanReach_Server.Scriptable;
using static TitanReach_Server.Plugins.AI.StoneTitan.State;

namespace TitanReach_Server.Plugins.AI
{
    class StoneTitan : Behaviour
    {

        public enum State { IDLE, SHIELDED, ATTACKING };
        public override int NpcID() { return 99; }
        public override Type RegisterStates() { return typeof(State); }

        public override int OnAttack(Player attacker, int damage)
        {
            return GetState() == (uint)SHIELDED ? 0 : -1;
        }

        public override void OnState(int state)
        {
            switch ((State)state)
            {
                case IDLE:
                    // Server.Log("Idle mode");
                    break;

                case SHIELDED:
                    //  Server.Log("Entered shield mode");
                    break;
            }
        }



        public override void RegisterTransitions()
        {
            Transition(ALL_STATES, (uint)SHIELDED, () =>
            {
                return npc.HealthPercentage() >= 60 && npc.HealthPercentage() <= 80;
            });
            Transition(ALL_STATES, (uint)IDLE, () =>
            {
                return GetState() != (uint)SHIELDED;
            });

        }

        public override void Update()
        {
        }
    }
}
