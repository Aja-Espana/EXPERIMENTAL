using UnityEngine;
using Schema;

namespace Game.AI.Birds
{
    [Name("Bird/Tweet to Void")]
    public class BirdTweetVoidAction : Action
    {
        public string tweetPatternId = "Pattern_A";
        public float broadcastRadius = 12f;
        public LayerMask birdLayer;

        [Header("Audio")]
        public AudioClip[] tweetClips;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator animator = agent.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("TriggerTweet");
            }

            AudioSource audio = agent.GetComponent<AudioSource>();
            if (audio != null && tweetClips != null && tweetClips.Length > 0)
            {
                AudioClip clipToPlay = tweetClips[Random.Range(0, tweetClips.Length)];
                audio.PlayOneShot(clipToPlay);
            }

            ParticleSystem particle = agent.GetComponent<ParticleSystem>();
            if (particle != null) 
            {
                particle.Play();
            }

            // Inside BirdTweetVoidAction.cs Tick()
            BirdMessage outgoingMessage = new BirdMessage
            {
                actionType = BirdActionType.MoveToLocation,
                targetPosition = agent.transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)),
                groupID = 101,
                urgency = 1.0f
            };

            Collider[] hits = Physics.OverlapSphere(agent.transform.position, broadcastRadius, birdLayer);
            foreach (var hit in hits)
            {
                if (hit.gameObject != agent.gameObject)
                {
                    Bird neighborBird = hit.GetComponent<Bird>();
                    if (neighborBird != null)
                    {
                        neighborBird.ReceiveTweet(outgoingMessage, agent.transform.position);
                    }
                }
            }

            return NodeStatus.Success;
        }
    }
}