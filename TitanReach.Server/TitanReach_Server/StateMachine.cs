using System.Collections.Generic;

public delegate bool TransitionCheck();
public delegate void StateFunction();

public class State
{

    private StateFunction m_onEntry;
    private StateFunction m_onExit;
    private StateFunction m_on;

    public State()
    {

    }

    public State(StateFunction p_onEntry, StateFunction p_on, StateFunction p_exit)
    {
        SetOnEntry(p_onEntry);
        SetOn(p_on);
        SetOnExit(p_exit);
    }

    public void SetOnEntry(StateFunction p_function)
    {
        m_onEntry = p_function;
    }

    public void SetOnExit(StateFunction p_function)
    {
        m_onExit = p_function;
    }
    public void SetOn(StateFunction p_function)
    {
        m_on = p_function;
    }

    public void OnEntry()
    {
        if (m_onEntry != null)
        {
            m_onEntry.Invoke();
        }
    }
    public void OnExit()
    {
        if (m_onExit != null)
        {
            m_onExit.Invoke();
        }
    }
    public void On()
    {
        if (m_on != null)
        {
            m_on.Invoke();
        }
    }
}

public class Transition
{
    private uint m_nextStateId;
    private TransitionCheck m_transitionCheck;

    public Transition(uint p_nextState, TransitionCheck p_transitionCheck)
    {
        m_transitionCheck = p_transitionCheck;
        m_nextStateId = p_nextState;
    }

    static Transition create(uint p_nextState, TransitionCheck p_transitionCheck)
    {
        return new Transition(p_nextState, p_transitionCheck);
    }

    public uint NextState()
    {
        return m_nextStateId;
    }

    public bool ShouldTransition()
    {
        if (m_transitionCheck != null)
        {
            return m_transitionCheck.Invoke();
        }

        return false;
    }

}
public class StateMachine
{
    private uint m_previousState;
    private uint m_currentState;

    private SortedDictionary<uint, State> m_states;
    private SortedDictionary<uint, List<Transition>> m_transitions;

    public uint[] GetStates()
    {
        uint[] temp = new uint[m_states.Keys.Count];
        m_states.Keys.CopyTo(temp, 0);
        return temp;
    }

    public StateMachine()
    {
        m_states = new SortedDictionary<uint, State>();
        m_transitions = new SortedDictionary<uint, List<Transition>>();
    }

    public static StateMachine Create()
    {
        return new StateMachine();
    }

    public void SetInitialState(uint p_stateId)
    {
        m_currentState = p_stateId;
    }
    public void RegisterState(uint p_stateId, State p_state)
    {
        if (m_states.ContainsKey(p_stateId)) //hmmm
        {
            m_states[p_stateId] = p_state;
        }
        else
        {
            m_states[p_stateId] = p_state;
        }
    }
    public bool RegisterTransition(uint p_stateId, uint p_targetId, TransitionCheck p_check)
    {
        if (m_states.ContainsKey(p_stateId) && m_states.ContainsKey(p_targetId))
        {
            if (m_transitions.ContainsKey(p_stateId))
            {
                m_transitions[p_stateId].Add(new Transition(p_targetId, p_check));
            }
            else
            {
                m_transitions[p_stateId] = new List<Transition>();
                m_transitions[p_stateId].Add(new Transition(p_targetId, p_check));
            }
            return true;
        }
        return false;
    }

    public bool RegisterMultipleTransition(uint[] p_stateIds, uint p_targetId, TransitionCheck p_check)
    {
        for (int i = 0; i < p_stateIds.Length; i++)
        {
            uint stateID = p_stateIds[i];
            RegisterTransition(stateID, p_targetId, p_check);

            if (i == p_stateIds.Length - 1) return true;
        }

        return false;
    }

    public bool RegisterOverrideTransition(uint p_totalStateCount, uint p_targetID, TransitionCheck p_check)
    {
        for (uint stateId = 0; stateId < p_totalStateCount; stateId++)
        {
            if (stateId != p_targetID)
            {
                RegisterTransition(stateId, p_targetID, p_check);
            }

            if (stateId == p_totalStateCount - 1) return true;
        }

        return false;
    }

    public void Run()
    {
        checkStateTransition();
        runCurrentState();
    }

    public void RunCurrentState()
    {
        runCurrentState();
    }

    public void ForceRunInitialState()
    {
        runCurrentStateOnEntry();
    }

    public uint GetCurrentState()
    {
        return m_currentState;
    }

    private void checkStateTransition()
    {
        if (m_transitions.ContainsKey(m_currentState))
        {
            List<Transition> stateTransitions = m_transitions[m_currentState];
            foreach (var transition in stateTransitions)
            {
                if (transition.ShouldTransition())
                {
                    changeState(transition);
                    break;
                }
            }
        }
    }

    private void changeState(Transition transition)
    {
        m_previousState = m_currentState;
        m_currentState = transition.NextState();

        m_states[m_previousState].OnExit();
        m_states[m_currentState].OnEntry();
    }

    public void OverrideState(uint newState)
    {
        m_previousState = m_currentState;
        m_currentState = newState;

        m_states[m_previousState].OnExit();
        m_states[m_currentState].OnEntry();
    }

    private void runCurrentState()
    {
        if (m_states.ContainsKey(m_currentState))
        {
            m_states[m_currentState].On();
        }
    }

    private void runCurrentStateOnEntry()
    {
        if (m_states.ContainsKey(m_currentState))
        {
            m_states[m_currentState].OnEntry();
        }
    }
}




