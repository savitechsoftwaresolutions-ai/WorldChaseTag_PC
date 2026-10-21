using System;
using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.Combat
{
    public class WCTTagSystem : MonoBehaviour
    {
        public static WCTTagSystem Instance { get; private set; }

        [Header("Tag Reach & Detection")]
        [SerializeField] private float tagReachDistance = 1.35f;
        [SerializeField] private float tagSweepRadius = 0.35f;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private LayerMask obstacleOcclusionLayers = ~0;

        [Header("References")]
        [SerializeField] private Transform activeEvaderTransform;
        [SerializeField] private Transform activeChaserTransform;

        public event Action<Vector3> OnTagVisualEffect;
        public event Action OnTagAttempted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SetActiveTargets(Transform chaser, Transform evader)
        {
            activeChaserTransform = chaser;
            activeEvaderTransform = evader;
        }

        public bool ExecuteTagAttempt(Vector3 origin, Vector3 forward)
        {
            OnTagAttempted?.Invoke();

            if (WCTMatchController.Instance == null || !WCTMatchController.Instance.IsChaseActive)
            {
                return false;
            }

            // Continuous capsule sweep forward to prevent fast-movement tunneling
            Ray ray = new Ray(origin, forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, tagSweepRadius, tagReachDistance, hitLayers, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                // Check if hit object is the Evader
                if (hit.collider.CompareTag("Evader") || (activeEvaderTransform != null && hit.transform == activeEvaderTransform))
                {
                    // Check line of sight (not occluded by walls or obstacles)
                    Vector3 directionToEvader = (hit.point - origin).normalized;
                    float distanceToHit = Vector3.Distance(origin, hit.point);

                    if (Physics.Raycast(origin, directionToEvader, out RaycastHit wallHit, distanceToHit - 0.05f, obstacleOcclusionLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (wallHit.collider != hit.collider)
                        {
                            // Tag blocked by obstacle
                            continue;
                        }
                    }

                    // Valid tag confirmed!
                    OnTagVisualEffect?.Invoke(hit.point);
                    WCTMatchController.Instance.ResolveTag(WCTMatchController.Instance.TimeRemaining);
                    return true;
                }
            }

            // Proximity check fallback for near-contact
            if (activeEvaderTransform != null && activeChaserTransform != null)
            {
                float dist = Vector3.Distance(activeChaserTransform.position, activeEvaderTransform.position);
                if (dist <= 1.25f)
                {
                    Vector3 losOrigin = activeChaserTransform.position + Vector3.up * 1.0f;
                    Vector3 losTarget = activeEvaderTransform.position + Vector3.up * 1.0f;
                    Vector3 toEvader = losTarget - losOrigin;

                    if (!Physics.Raycast(losOrigin, toEvader.normalized, toEvader.magnitude - 0.1f, obstacleOcclusionLayers, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 contactPoint = (losOrigin + losTarget) * 0.5f;
                        OnTagVisualEffect?.Invoke(contactPoint);
                        WCTMatchController.Instance.ResolveTag(WCTMatchController.Instance.TimeRemaining);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
