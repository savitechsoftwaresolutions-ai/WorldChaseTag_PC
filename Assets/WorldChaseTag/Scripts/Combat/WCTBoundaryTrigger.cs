using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.Combat
{
    [RequireComponent(typeof(BoxCollider))]
    public class WCTBoundaryTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float arenaWidth = 12.0f; // Official Quad is 12m x 12m
        [SerializeField] private float arenaLength = 12.0f;
        [SerializeField] private float arenaHeight = 6.0f;
        [SerializeField] private bool autoConfigureCollider = true;

        private BoxCollider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerCollider.isTrigger = true;

            if (autoConfigureCollider)
            {
                triggerCollider.size = new Vector3(arenaWidth, arenaHeight, arenaLength);
                triggerCollider.center = new Vector3(0f, arenaHeight * 0.5f, 0f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (WCTMatchController.Instance == null || !WCTMatchController.Instance.IsChaseActive) return;

            // If Evader exits boundary -> Out of bounds violation
            if (other.CompareTag("Evader"))
            {
                WCTMatchController.Instance.ResolveBoundaryViolation(WCTMatchController.Instance.ActiveEvaderTeam);
            }
        }
    }
}
