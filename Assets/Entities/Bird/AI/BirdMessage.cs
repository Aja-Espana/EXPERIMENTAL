using UnityEngine;

public enum BirdActionType
{
    None,
    MoveToLocation,
    FollowLeader,
    GatherAtCenter
}

[System.Serializable]
public struct BirdMessage
{
    public BirdActionType actionType; // 1-4 bytes depending on enum size
    public Vector3 targetPosition;     // 12 bytes
    public int groupID;                // 4 bytes
    public float urgency;              // 4 bytes
    
}