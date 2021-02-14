using System;

namespace MelonECS.FSM
{
    public class EcsStateMachine<TEnum> : System, IStateMachine<TEnum> where TEnum : Enum
    {
        private readonly StateMachine<TEnum> stateMachine = new StateMachine<TEnum>();
        
        public override void Run()
        {
            stateMachine.Update();
        }

        public IStateMachine<TEnum> AddState<TState>(TEnum stateEnum) where TState : IState, new()
        {
            stateMachine.AddState<TState>(stateEnum);
            return this;
        }

        public void ChangeState(TEnum stateEnum)
        {
            stateMachine.ChangeState(stateEnum);
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}