using Mandible.Entities;
using Mandible.Core;

namespace Mandible.Core
{
    public interface ICapability
    {
        void Initialize(IAgent agent);
    }
}