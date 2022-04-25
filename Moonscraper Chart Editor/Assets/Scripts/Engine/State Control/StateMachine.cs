// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace MoonscraperEngine
{
    public class StateMachine
    {
        public interface IState
        {
            void Enter();
            void Update();
            void Exit();
        }

        IState _currentState = null;
        public IState currentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                ChangeState(value);
            }
        }

        void ChangeState(IState state)
        {
            if (currentState != null)
                currentState.Exit();

            _currentState = state;

            if (currentState != null)
                currentState.Enter();
        }

        public void Update()
        {
            if (currentState != null)
                currentState.Update();
        }
    }

    public class StateMachine<Params>
    {
        public interface IState
        {
            void Enter(Params parameters);
            void Update(Params parameters);
            void Exit(Params parameters);
        }

        public IState currentState { get; private set; }

        void ChangeState(IState state, Params parameters)
        {
            if (currentState != null)
                currentState.Exit(parameters);

            currentState = state;

            if (currentState != null)
                currentState.Enter(parameters);
        }

        public void Update(Params parameters)
        {
            if (currentState != null)
                currentState.Update(parameters);
        }
    }
}
