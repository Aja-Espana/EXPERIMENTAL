using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bird : Entity
{
    [Header("Flocking Settings")]
    public Transform flockCenterAnchor; // Center of mass they shouldn't stray far from
    public float maxDistanceFromCenter = 15f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector3 currentMoveTarget;
    private bool isFollowingLeader = false;
    private Transform currentLeader;

    public bool HasReachedTarget => currentMoveTarget != Vector3.zero && Vector3.Distance(transform.position, currentMoveTarget) <= 0.5f;

    protected override void Start()
    {

    }

    protected override void Update()
    {
        HandleMovementAndFacing();
    }

    private void HandleMovementAndFacing()
    {
        // Enforce center of mass anchor constraint if no active specific target
        Vector3 targetPos = currentMoveTarget;
        
        if (flockCenterAnchor != null && Vector3.Distance(transform.position, flockCenterAnchor.position) > maxDistanceFromCenter)
        {
            // Pull back toward center of mass gently
            targetPos = flockCenterAnchor.position;
        }

        if (isFollowingLeader && currentLeader != null)
        {
            targetPos = currentLeader.position;
        }

        // Move towards target position
        if (targetPos != Vector3.zero)
        {
            Vector3 direction = (targetPos - transform.position);
            direction.y = 0; // Keep flat on ground/plane

            if (direction.magnitude > 0.5f)
            {
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;

                // Smoothly face the movement direction or target
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void SetNavigationTarget(Vector3 destination)
    {
        currentMoveTarget = destination;
        isFollowingLeader = false;
    }

    public void ClearNavigationTarget()
    {
        currentMoveTarget = Vector3.zero;
        isFollowingLeader = false;
        currentLeader = null;
    }

    public void FollowTargetLeader(Transform leader)
    {
        currentLeader = leader;
        isFollowingLeader = true;
    }

    // Tweets
    private Queue<BirdMessage> tweetQueue = new Queue<BirdMessage>();
    public bool HasUnreadTweet => tweetQueue.Count > 0;

    public void ReceiveTweet(BirdMessage message, Vector3 sourcePosition)
    {
        tweetQueue.Enqueue(message);
    }

    public BirdMessage ConsumeLatestTweet()
    {
        if (tweetQueue.Count > 0)
        {
            return tweetQueue.Dequeue();
        }
        return default;
    }
}
