using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Combat;

namespace WorldChaseTag.AI
{
    [RequireComponent(typeof(WCTAgentMotor))]
    public class WCTChaserAI : MonoBehaviour
    {
        [Header("Target & Vision")]
        [SerializeField] private Transform evaderTransform;
        [SerializeField] private LayerMask obstacleLayer = ~0;

        [Header("Tactics & Interception")]
        [SerializeField] private float tagReach = 2.0f;
        [SerializeField] private float leadTimePrediction = 0.6f;
        [SerializeField] private float pathRecalculateRate = 0.12f;

        private WCTAgentMotor motor;
        private Vector3 lastEvaderPosition;
        private Vector3 estimatedEvaderVelocity;
        private float nextDecisionTime = 0f;
        private Vector3 currentInterceptionPoint;

        public bool IsActiveChaser { get; set; } = false;

        private void Awake()
        {
            motor = GetComponent<WCTAgentMotor>();
        }

        public void SetTarget(Transform target)
        {
            evaderTransform = target;
            if (evaderTransform != null)
            {
                lastEvaderPosition = evaderTransform.position;
            }
        }

        private void Update()
        {
            if (WCTMatchController.Instance == null || !WCTMatchController.Instance.IsChaseActive || !IsActiveChaser)
            {
                return;
            }

            if (evaderTransform == null) return;

            // Estimate Evader velocity
            float dt = Time.deltaTime;
            if (dt > 0.001f)
            {
                Vector3 currentEvaderPos = evaderTransform.position;
                estimatedEvaderVelocity = (currentEvaderPos - lastEvaderPosition) / dt;
                lastEvaderPosition = currentEvaderPos;
            }

            // Periodic tactical decision
            if (Time.time >= nextDecisionTime)
            {
                nextDecisionTime = Time.time + pathRecalculateRate;
                CalculateInterceptionPoint();
            }

            // Move motor towards interception point
            motor.MoveToward(currentInterceptionPoint, true);

            // Check if within tag reach
            float distanceToEvader = Vector3.Distance(transform.position, evaderTransform.position);
            if (distanceToEvader <= tagReach)
            {
                // Attempt tag
                Vector3 tagOrigin = transform.position + Vector3.up * 1.0f;
                Vector3 forward = (evaderTransform.position - transform.position).normalized;
                WCTTagSystem.Instance?.ExecuteTagAttempt(tagOrigin, forward);
            }
        }

        private void CalculateInterceptionPoint()
        {
            AIDifficulty diff = WCTMatchController.Instance != null ? WCTMatchController.Instance.Difficulty : AIDifficulty.Normal;

            float leadFactor = leadTimePrediction;
            float errorNoise = 0f;

            switch (diff)
            {
                case AIDifficulty.Easy:
                    leadFactor = 0.2f;
                    errorNoise = 1.2f;
                    break;
                case AIDifficulty.Normal:
                    leadFactor = 0.5f;
                    errorNoise = 0.4f;
                    break;
                case AIDifficulty.Hard:
                    leadFactor = 0.8f;
                    errorNoise = 0.1f;
                    break;
                case AIDifficulty.Expert:
                    leadFactor = 1.0f;
                    errorNoise = 0.0f;
                    break;
            }

            Vector3 projectedPos = evaderTransform.position + estimatedEvaderVelocity * leadFactor;
            projectedPos.y = 0.5f;

            // Clamp inside legal Quad arena (12m x 12m perimeter)
            projectedPos.x = Mathf.Clamp(projectedPos.x, -5.5f, 5.5f);
            projectedPos.z = Mathf.Clamp(projectedPos.z, -5.5f, 5.5f);

            // If line of sight is clear, head straight for predicted interception
            Vector3 origin = transform.position + Vector3.up * 1.0f;
            Vector3 target = evaderTransform.position + Vector3.up * 1.0f;
            Vector3 toTarget = target - origin;

            if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, toTarget.magnitude, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                // Line of sight blocked by obstacle -> cut around obstacle edge
                Vector3 flankOffset = Vector3.Cross(hit.normal, Vector3.up) * 1.8f;
                Vector3 flankA = hit.point + flankOffset;
                Vector3 flankB = hit.point - flankOffset;

                // Pick flank closest to projected target
                currentInterceptionPoint = Vector3.Distance(flankA, projectedPos) < Vector3.Distance(flankB, projectedPos) ? flankA : flankB;
            }
            else
            {
                // Clear direct route to predicted target
                currentInterceptionPoint = projectedPos;
            }

            // Apply difficulty noise
            if (errorNoise > 0.01f)
            {
                Vector2 noise = UnityEngine.Random.insideUnitCircle * errorNoise;
                currentInterceptionPoint += new Vector3(noise.x, 0f, noise.y);
            }
        }
    }
}
