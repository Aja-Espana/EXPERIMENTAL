using UnityEngine;
using UnityEditor;

namespace Mandible.Core.Data
{
    public abstract class RuntimeObject : ScriptableObject
    {
        protected bool isInstance = false;

        // Getters
        public bool IsInstance => isInstance;

        /// <summary>
        /// Returns a runtime clone of the actual derived SO type
        /// </summary>

        public RuntimeObject CreateRuntimeInstance()
        {
            RuntimeObject instance = Instantiate(this);
            instance.OnInstantiate();

            return instance;
        }

        private void OnInstantiate()
        {
            isInstance = true;
        }
    }
}
