using UnityEngine;
using Mandible.Core;

namespace Mandible.Entities
{
    public sealed class ThrowCapability : IThrowCapability
    {
        [SerializeField] IAgent agent;
        [SerializeField] Transform throwOrigin;

        public void Initialize(IAgent agent)
        {
            this.agent = agent;
        }

        public void Throw(ThrowActionData data)
        {
            if (data == null || data.projectilePrefab == null) return;

            var instance = Object.Instantiate(data.projectilePrefab, agent.transform.position + data.spawnOffset, agent.transform.rotation);

            if (instance.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = agent.GetLookDirection();
                rb.AddForce(dir.normalized * data.force, ForceMode.Impulse);
            }

            // animation triggering should be capability based
            /*
            FPSProceduralController fpsController = (agent as MonoBehaviour).GetComponent<FPSProceduralController>();
            //int throwTime = fpsController.GetAnimationDuration("Throw", "UpperBody");
            fpsController.Throw(data);
            */

            
            Animator anim = (agent as MonoBehaviour).GetComponent<Animator>();
            if(anim != null)
            {
                int layerIndex = anim.GetLayerIndex("UpperBody");
                anim.Play("Throw", layerIndex, 0f);
            }
        }
    }
}
