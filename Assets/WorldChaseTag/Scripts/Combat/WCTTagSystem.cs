using System;
using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.Combat
{
    public class WCTTagSystem : MonoBehaviour
    {
        public static WCTTagSystem Instance { get; private set; }

        [Header("Tag Reach & Detection")]
        [SerializeField] private float tagReachDistance = 2.2f;
        [SerializeField] private float tagSweepRadius = 0.55f;
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

            // Audio feedback for reach attempt
            WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayVaultWhoosh();

            // 1. Continuous capsule sweep forward to prevent fast-movement tunneling
            Ray ray = new Ray(origin, forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, tagSweepRadius, tagReachDistance, hitLayers, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                // Check if hit object is the Evader (root or child)
                bool isEvader = false;
                if (activeEvaderTransform != null)
                {
                    if (hit.transform == activeEvaderTransform || hit.transform.IsChildOf(activeEvaderTransform) || hit.transform.root == activeEvaderTransform.root)
                    {
                        isEvader = true;
                    }
                }

                if (isEvader)
                {
                    // Check line of sight (not occluded by walls or obstacles)
                    Vector3 directionToEvader = (hit.point - origin).normalized;
                    float distanceToHit = Vector3.Distance(origin, hit.point);

                    RaycastHit[] occludes = Physics.RaycastAll(origin, directionToEvader, distanceToHit - 0.1f, obstacleOcclusionLayers, QueryTriggerInteraction.Ignore);
                    bool blocked = false;
                    foreach (var occ in occludes)
                    {
                        if (activeChaserTransform != null && (occ.transform == activeChaserTransform || occ.transform.IsChildOf(activeChaserTransform))) continue;
                        if (activeEvaderTransform != null && (occ.transform == activeEvaderTransform || occ.transform.IsChildOf(activeEvaderTransform))) continue;
                        blocked = true;
                        break;
                    }

                    if (blocked) continue;

                    // Valid tag confirmed!
                    OnTagVisualEffect?.Invoke(hit.point);
                    WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayTagSound();
                    WCTMatchController.Instance.ResolveTag(WCTMatchController.Instance.TimeRemaining);
                    return true;
                }
            }

            // 2. Proximity check fallback for near-contact within forward 160-degree arc
            if (activeEvaderTransform != null && activeChaserTransform != null)
            {
                Vector3 chaserPos = activeChaserTransform.position;
                Vector3 evaderPos = activeEvaderTransform.position;
                float dist = Vector3.Distance(chaserPos, evaderPos);

                if (dist <= tagReachDistance + 0.3f)
                {
                    Vector3 toEvader = evaderPos - chaserPos;
                    toEvader.y = 0f;
                    float angle = Vector3.Angle(activeChaserTransform.forward, toEvader.normalized);

                    // Must be within 80 degrees of forward direction
                    if (angle <= 80f)
                    {
                        Vector3 losOrigin = chaserPos + Vector3.up * 1.0f;
                        Vector3 losTarget = evaderPos + Vector3.up * 1.0f;
                        Vector3 losDir = losTarget - losOrigin;

                        RaycastHit[] occludes = Physics.RaycastAll(losOrigin, losDir.normalized, losDir.magnitude - 0.1f, obstacleOcclusionLayers, QueryTriggerInteraction.Ignore);
                        bool blocked = false;
                        foreach (var occ in occludes)
                        {
                            if (occ.transform == activeChaserTransform || occ.transform.IsChildOf(activeChaserTransform)) continue;
                            if (occ.transform == activeEvaderTransform || occ.transform.IsChildOf(activeEvaderTransform)) continue;
                            blocked = true;
                            break;
                        }

                        if (!blocked)
                        {
                            Vector3 contactPoint = (losOrigin + losTarget) * 0.5f;
                            OnTagVisualEffect?.Invoke(contactPoint);
                            WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayTagSound();
                            WCTMatchController.Instance.ResolveTag(WCTMatchController.Instance.TimeRemaining);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
