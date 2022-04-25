// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine
{
    public class Event
    {
        public delegate void EventCallback();
        List<EventCallback> callbackList = new List<EventCallback>();

        public void Register(EventCallback callbackFn)
        {
            callbackList.Add(callbackFn);
        }

        public void Deregister(EventCallback callbackFn)
        {
            callbackList.Remove(callbackFn);
        }

        public void Fire()
        {
            foreach (EventCallback function in callbackList)
                function();
        }

        public void Clear()
        {
            callbackList.Clear();
        }
    }

    public class Event<Params>
    {
        public delegate void EventCallback(in Params parameters);
        List<EventCallback> callbackList = new List<EventCallback>();

        public void Register(EventCallback callbackFn)
        {
            callbackList.Add(callbackFn);
        }

        public bool Deregister(EventCallback callbackFn)
        {
            return callbackList.Remove(callbackFn);
        }

        public void Fire(in Params parameters)
        {
            foreach (EventCallback function in callbackList)
                function(in parameters);
        }

        public void Clear()
        {
            callbackList.Clear();
        }
    }
}
