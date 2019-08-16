using System.Collections;
using System.Collections.Generic;

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
