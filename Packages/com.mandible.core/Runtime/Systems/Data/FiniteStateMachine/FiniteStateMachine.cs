using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Mandible.Core.Data;

namespace Mandible.Core.Data
{
    public class FiniteStateMachine : MonoBehaviour { }

    [System.Serializable]
    public class FiniteState
    {
        [SerializeField] public string name = "New State";
        [SerializeField] UnityEvent stateEvent = new UnityEvent();

        public FiniteState(string name = "New State", UnityEvent newEvent = default)
        {
            this.name = name;
            this.stateEvent = newEvent;
        }

        public void OnEventUpdate()
        {
            stateEvent?.Invoke();
        }
    }
}


