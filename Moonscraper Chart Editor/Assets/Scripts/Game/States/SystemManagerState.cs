using System.Collections;
using System.Collections.Generic;

// A type of state that manages a series of systems
public class SystemManagerState : StateMachine.IState
{
    // A system that recieves heartbeats if the current state is the one it's registered in
    public abstract class System
    {
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    List<System> registeredSystems = new List<System>();

    public void AddSystem(System system)
    {
        registeredSystems.Add(system);
    }

    public void AddSystems(IList<System> systems)
    {
        registeredSystems.AddRange(systems);
    }

    public void RemoveSystem(System system)
    {
        registeredSystems.Remove(system);
    }

    public virtual void Enter()
    {
        foreach (System system in registeredSystems)
            system.Enter();
    }

    public virtual void Exit()
    {
        foreach (System system in registeredSystems)
            system.Exit();
    }

    public virtual void Update()
    {
        foreach (System system in registeredSystems)
            system.Update();
    }
}
