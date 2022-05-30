using System;
using TitanReach_Server.Model;

namespace TitanReach_Server.Scriptable
{
    public abstract class Behaviour
    {

        public Npc npc;
        private StateMachine machine = new StateMachine();
        private Type statetype;

        public void InitBehaviour(Npc npc)
        {
            this.npc = npc;
            Type enu = RegisterStates();
            if (enu.IsEnum)
            {
                statetype = enu;
                System.Array enumValues = System.Enum.GetValues(enu);

                for (int i = 0; i < enumValues.Length; i++)
                {
                    int value = (int)enumValues.GetValue(i);
                    State stat = new State();
                    //stat.SetOnEntry(StateEntry);
                    stat.SetOn(StateMiddle);
                    // stat.SetOnExit(StateExit);

                    machine.RegisterState((uint)value, stat);
                }
            }

            RegisterTransitions();
            OnLoad();
            Server.Instance.LoopedDelay(200, (timer, arg) =>
            {
                machine.Run();
                Update();
            });

        }


        private void StateMiddle()
        {
            OnState(GetState());
        }

        public int GetState()
        {
            return (int)machine.GetCurrentState();
        }

        public const uint ALL_STATES = uint.MaxValue;
        public void Transition(uint state1, uint state2, TransitionCheck check)
        {
            if (state1 == uint.MaxValue)
            {
                foreach (uint num in machine.GetStates())
                {
                    if (num == state2)
                        continue;
                    machine.RegisterTransition(num, state2, check);
                }
                return;
            }
            machine.RegisterTransition(state1, state2, check);
        }


        /**
        * 
        *  Abstract Functions Here
        *  - AI Scripts are forced to inherit these functions
        *  
        * */
        public abstract int NpcID();
        public abstract void OnState(int state);

        public abstract Type RegisterStates();

        public abstract void RegisterTransitions();

        // will get called every 200ms
        public abstract void Update();


        /**
         * 
         *  Virtual Functions Here
         *  - AI Scripts can override these and use them as a listener
         * 
         * */
        public virtual void OnAttack() { }
        public virtual void OnDeath() { }
        public virtual void OnLoad() { }
        public virtual void OnSpawn() { }
        // OnAttack - damage variable is the amount of damage done to this NPC. -1 is normal damage
        public virtual int OnAttack(Player attacker, int damage) { return -1; }

    }
}
