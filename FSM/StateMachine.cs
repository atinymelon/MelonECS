using System;
using System.Collections.Generic;

namespace MelonECS.FSM
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void Update();
    }
    
    public interface IStateMachine<TEnum> where TEnum : Enum
    {
        IStateMachine<TEnum> AddState<TState>(TEnum stateEnum) where TState : IState, new();
        void ChangeState(TEnum stateEnum);
        void Update();
    }
    
    public class StateMachine<TEnum> : IStateMachine<TEnum> where TEnum : Enum
    {
        private readonly Dictionary<TEnum, IState> states = new Dictionary<TEnum, IState>();
        private IState currentState;
        private IState pendingState;
        
        public IStateMachine<TEnum> AddState<TState>(TEnum stateEnum) where TState : IState, new()
        {
            states.Add(stateEnum, new TState());
            return this;
        }

        public void ChangeState(TEnum stateEnum)
        {
            pendingState = states[stateEnum];
        }

        public void Update()
        {
            currentState?.Update();

            if (pendingState != null)
            {
                currentState?.OnExit();
                currentState = pendingState;
                currentState?.OnEnter();
                pendingState = null;
            }
        }
    }
}