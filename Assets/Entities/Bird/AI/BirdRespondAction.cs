using UnityEngine;
using Schema;

namespace Game.AI.Birds
{
    [Name("Bird/Respond to Tweet")]
    public class BirdRespondAction : Action
    {
        public BlackboardEntrySelector<BirdMessage> interceptedMessageEntry;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            BirdMessage message = interceptedMessageEntry != null ? interceptedMessageEntry.value : default;
            Bird bird = agent.GetComponent<Bird>();

            if (bird != null)
            {
                // Execute instruction payload
                if (message.actionType == BirdActionType.MoveToLocation)
                {
                    bird.SetNavigationTarget(message.targetPosition);
                }
            }

            return NodeStatus.Success;
        }
    }
}