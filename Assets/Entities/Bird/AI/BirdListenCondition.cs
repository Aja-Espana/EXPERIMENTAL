using UnityEngine;
using Schema;

namespace Game.AI.Birds
{
    [Name("Bird/Listen For Tweets")]
    public class BirdListenCondition : Action
    {
        public BlackboardEntrySelector<BirdMessage> interceptedMessageEntry;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Bird bird = agent.GetComponent<Bird>();
            if (bird != null && bird.HasUnreadTweet)
            {
                BirdMessage message = bird.ConsumeLatestTweet();

                if (interceptedMessageEntry != null)
                {
                    interceptedMessageEntry.value = message;
                }

                // Debug log the incoming instruction struct details
                //Debug.Log($"{agent.name} heard a tweet! Action: {message.actionType}, Target: {message.targetPosition}, Group ID: {message.groupID}, Urgency: {message.urgency}");

                return NodeStatus.Success; 
            }

            return NodeStatus.Failure; 
        }
    }
}