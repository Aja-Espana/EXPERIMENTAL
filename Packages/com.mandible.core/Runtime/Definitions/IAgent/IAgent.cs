using UnityEngine;
using Mandible.Core;

namespace Mandible.Core
{
    public interface IAgent
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        //Capabilities
        T GetCapability<T>() where T : ICapability;
        void AddCapability<T>() where T : ICapability;
        
        IInputSystem Input { get; }
        Vector3 GetLookDirection();

        void ApplyImpulse(Vector3 impulse);
        void CancelGravity();
    }
}
