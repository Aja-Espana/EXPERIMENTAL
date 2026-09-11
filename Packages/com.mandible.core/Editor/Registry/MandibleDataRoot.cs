using UnityEngine;

namespace Mandible.Registry
{
    [CreateAssetMenu(fileName = "MandibleDataRoot", menuName = "Mandible/Core/Data Root", order = 0)]
    public sealed class MandibleDataRoot : ScriptableObject
    {
        [HideInInspector] public int schemaVersion = 1;
    }
}
