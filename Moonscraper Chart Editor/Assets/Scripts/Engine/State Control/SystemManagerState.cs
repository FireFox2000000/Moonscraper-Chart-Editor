// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace MoonscraperEngine
{
    // A type of state that manages a series of systems
    public class SystemManagerState : StateMachine.IState
    {
        // A system that recieves heartbeats if the current state is the one it's registered in
        public interface ISystem
        {
            void SystemEnter();
            void SystemUpdate();
            void SystemExit();
        }

        // Standard system class, updates when the state machine updates
        public abstract class System : ISystem
        {
            public virtual void SystemEnter() { }
            public virtual void SystemUpdate() { }
            public virtual void SystemExit() { }
        }

        // A system that has the properties of a UnityEngine MonoBehaviour for finer update timing control (Update, LateUpdate, script execution order etc)
        public class MonoBehaviourSystem : UnityEngine.MonoBehaviour, ISystem
        {
            public void SystemEnter()
            {
                enabled = true;
            }

            public void SystemExit()
            {
                enabled = false;
            }

            public void SystemUpdate() { }
        }

        List<ISystem> registeredSystems = new List<ISystem>();
        bool hasEntered = false;

        public void AddSystem(ISystem system)
        {
            registeredSystems.Add(system);

            if (hasEntered)
                system.SystemEnter();
        }

        public void AddSystems(IList<ISystem> systems)
        {
            foreach (ISystem system in systems)
                AddSystem(system);
        }

        public void RemoveSystem(ISystem system)
        {
            registeredSystems.Remove(system);
        }

        public virtual void Enter()
        {
            hasEntered = true;

            foreach (ISystem system in registeredSystems)
                system.SystemEnter();
        }

        public virtual void Exit()
        {
            foreach (ISystem system in registeredSystems)
                system.SystemExit();

            hasEntered = false;
            registeredSystems.Clear();
        }

        public virtual void Update()
        {
            foreach (ISystem system in registeredSystems)
                system.SystemUpdate();
        }
    }
}
