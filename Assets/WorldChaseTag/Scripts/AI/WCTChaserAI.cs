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
        [SerializeField] private float tagReach = 1.75f;
        [SerializeField] private float leadTimePrediction = 0.5f;
        [SerializeField] private float pathRecalculateRate = 0.12f;

        private WCTAgentMotor motor;
        private Vector3 lastEvaderPosition;
        private Vector3 estimatedEvaderVelocity;
        private float nextDecisionTime = 0f;
        private Vector3 currentInterceptionPoint;

        private float chaseStartTime = 0f;
        private bool wasChaseActiveLastFrame = false;
        private float startReactionDelay = 0.35f;

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
            bool isChaseActive = WCTMatchController.Instance != null && WCTMatchController.Instance.IsChaseActive && IsActiveChaser;

            if (isChaseActive && !wasChaseActiveLastFrame)
            {
                chaseStartTime = Time.time;
                AIDifficulty diff = WCTMatchController.Instance != null ? WCTMatchController.Instance.Difficulty : AIDifficulty.Normal;
                switch (diff)
                {
                    case AIDifficulty.Easy: startReactionDelay = 0.65f; break;
                    case AIDifficulty.Normal: startReactionDelay = 0.35f; break;
                    case AIDifficulty.Hard: startReactionDelay = 0.15f; break;
                    case AIDifficulty.Expert: startReactionDelay = 0.05f; break;
                }
                if (motor != null) motor.ApplyDifficultySettings(diff);
            }
            wasChaseActiveLastFrame = isChaseActive;

            if (!isChaseActive || evaderTransform == null)
            {
                return;
            }

            // Human Reaction delay at GO: gives Evader vital split-second head start to choose escape route
            if (Time.time < chaseStartTime + startReactionDelay)
            {
                return;
            }

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

            // Check if within tag reach with line of sight check
            float distanceToEvader = Vector3.Distance(transform.position, evaderTransform.position);
            if (distanceToEvader <= tagReach)
            {
                Vector3 tagOrigin = transform.position + Vector3.up * 1.0f;
                Vector3 targetHead = evaderTransform.position + Vector3.up * 1.0f;
                Vector3 forward = (evaderTransform.position - transform.position).normalized;

                // Strict line of sight check before attempting tag (no tagging through walls)
                if (!Physics.Linecast(tagOrigin, targetHead, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    WCTTagSystem.Instance?.ExecuteTagAttempt(tagOrigin, forward);
                }
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
