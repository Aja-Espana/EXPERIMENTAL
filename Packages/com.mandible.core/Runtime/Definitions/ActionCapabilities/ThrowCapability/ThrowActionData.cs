using UnityEngine;

namespace Mandible.Core
{
    [CreateAssetMenu(fileName = "ThrowActionData", menuName = "Actions/ThrowActionData")]
    public class ThrowActionData : ScriptableObject
    {
        [Header("Projectile")]
        public GameObject projectilePrefab;

        [Header("Ballistics")]
        public float force = 10f;
        public Vector3 spawnOffset = Vector3.zero;
        public float upwardBias = 0.2f;

        [Header("Animation")]
        public AnimationClip throwAnimation;
    }
}