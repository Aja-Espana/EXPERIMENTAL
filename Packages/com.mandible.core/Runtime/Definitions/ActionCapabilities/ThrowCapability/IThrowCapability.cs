using UnityEngine;

namespace Mandible.Core
{
    public interface IThrowCapability : ICapability
    {
        public void Throw(ThrowActionData data);
    }
}
