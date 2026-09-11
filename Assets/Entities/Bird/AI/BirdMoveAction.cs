using UnityEngine;
using Schema;

namespace Game.AI.Birds
{
    [Name("Bird/Move to Location")]
    public class BirdMoveAction : Action
    {
        public float arrivalThreshold = 0.5f;

        // Optional blackboard hook if you want an external target (like from a tweet message)
        public BlackboardEntrySelector<Vector3> targetPositionEntry;

        private class Memory
        {
            public Vector3 targetPosition;
            public bool hasDestination;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Memory mem = nodeMemory as Memory;
            Bird bird = agent.GetComponent<Bird>();
            Animator animator = agent.GetComponentInChildren<Animator>();

            if (bird == null) return NodeStatus.Failure;

            // Initialize destination if we don't have one
            if (!mem.hasDestination)
            {
                // Check if a blackboard entry provided a destination (e.g. from an intercepted tweet)
                if (targetPositionEntry != null && targetPositionEntry.value != Vector3.zero)
                {
                    mem.targetPosition = targetPositionEntry.value;
                }
                else
                {
                    // Fallback to random wander around current position or center anchor
                    Vector2 randomCircle = Random.insideUnitCircle * 5f;
                    mem.targetPosition = agent.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
                }

                // Tell the Bird component to start moving toward this target
                bird.SetNavigationTarget(mem.targetPosition);
                mem.hasDestination = true;
            }

            // Play walking animation
            if (animator != null)
            {
                animator.Play("Walk");
            }

            // Check if the bird has reached the destination using the Bird component's tracking
            float distance = Vector3.Distance(agent.transform.position, mem.targetPosition);
            if (distance <= arrivalThreshold || bird.HasReachedTarget)
            {
                mem.hasDestination = false;
                bird.ClearNavigationTarget();
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }
    }
}