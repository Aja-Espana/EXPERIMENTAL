using UnityEngine;
using Schema;

namespace Game.AI.Birds
{
    [Name("Bird/Idle")]
    public class BirdIdleAction : Action
    {
        public float minIdleDuration = 2f;
        public float maxIdleDuration = 5f;

        private class Memory
        {
            public float timer;
            public float targetDuration;
            public bool isInitialized;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Memory mem = nodeMemory as Memory;
            Animator animator = agent.GetComponentInChildren<Animator>();

            if (!mem.isInitialized)
            {
                mem.timer = 0f;
                mem.targetDuration = Random.Range(minIdleDuration, maxIdleDuration);
                mem.isInitialized = true;

                if (animator != null)
                {
                    animator.Play("Idle");
                }
            }

            mem.timer += Time.deltaTime;

            if (mem.timer >= mem.targetDuration)
            {
                mem.isInitialized = false; 
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }
    }
}