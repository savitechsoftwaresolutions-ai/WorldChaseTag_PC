using System.Collections.Generic;
using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.AI
{
    [RequireComponent(typeof(WCTAgentMotor))]
    public class WCTEvaderAI : MonoBehaviour
    {
        [Header("Target & Vision")]
        [SerializeField] private Transform chaserTransform;
        [SerializeField] private LayerMask obstacleLayer = ~0;

        [Header("Route Scoring Weights")]
        [SerializeField] private float weightDistance = 1.8f;      // w_d
        [SerializeField] private float weightSafety = 1.4f;        // w_s
        [SerializeField] private float weightEffectiveness = 1.2f; // w_e
        [SerializeField] private float weightRisk = 2.5f;          // w_r

        [Header("Tactics")]
        [SerializeField] private float decisionRate = 0.15f;
        [SerializeField] private float feintTriggerDistance = 3.2f;
        [SerializeField] private float arenaRadius = 5.2f; // Inside 12mx12m legal perimeter

        private WCTAgentMotor motor;
        private float nextDecisionTime = 0f;
        private Vector3 currentDestination;
        private List<Vector3> candidateWaypoints = new List<Vector3>();
        private float feintCooldown = 0f;

        public bool IsActiveEvader { get; set; } = false;

        private void Awake()
        {
            motor = GetComponent<WCTAgentMotor>();
            GenerateQuadWaypoints();
        }

        public void SetChaser(Transform chaser)
        {
            chaserTransform = chaser;
        }

        private void GenerateQuadWaypoints()
        {
            candidateWaypoints.Clear();
            // Generate grid of strategic Quad route waypoints
            float[] coords = new float[] { -4.2f, -2.1f, 0f, 2.1f, 4.2f };
            foreach (float x in coords)
            {
                foreach (float z in coords)
                {
                    candidateWaypoints.Add(new Vector3(x, 0.5f, z));
                }
            }
        }

        private void Update()
        {
            if (WCTMatchController.Instance == null || !WCTMatchController.Instance.IsChaseActive || !IsActiveEvader)
            {
                return;
            }

            if (chaserTransform == null) return;

            feintCooldown -= Time.deltaTime;

            if (Time.time >= nextDecisionTime)
            {
                nextDecisionTime = Time.time + decisionRate;
                EvaluateOptimalRoute();
            }

            // Execute movement
            motor.MoveToward(currentDestination, true);
        }

        private void EvaluateOptimalRoute()
        {
            float bestScore = float.MinValue;
            Vector3 bestPoint = transform.position;

            float timeRemaining = WCTMatchController.Instance != null ? WCTMatchController.Instance.TimeRemaining : 10f;
            bool lateClock = timeRemaining < 6.0f; // Clock awareness

            Vector3 chaserPos = chaserTransform.position;
            float distToChaser = Vector3.Distance(transform.position, chaserPos);

            // Trigger evasive feint when Chaser gets dangerously close
            if (distToChaser < feintTriggerDistance && feintCooldown <= 0f)
            {
                feintCooldown = 1.2f;
                // Feint perpendicular to Chaser approach
                Vector3 away = (transform.position - chaserPos).normalized;
                Vector3 perpendicular = Vector3.Cross(away, Vector3.up).normalized * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
                Vector3 feintTarget = transform.position + (away * 0.4f + perpendicular * 0.8f).normalized * 3.5f;

                feintTarget.x = Mathf.Clamp(feintTarget.x, -arenaRadius, arenaRadius);
                feintTarget.z = Mathf.Clamp(feintTarget.z, -arenaRadius, arenaRadius);
                currentDestination = feintTarget;
                return;
            }

            // Score all candidate Quad waypoints using: S(r) = w_d * D(r) + w_s * S_f(r) + w_e * E(r) - w_r * R(r)
            for (int i = 0; i < candidateWaypoints.Count; i++)
            {
                Vector3 pt = candidateWaypoints[i];

                // 1. D(r): Distance maintained from Chaser
                float d = Vector3.Distance(pt, chaserPos);

                // 2. S_f(r): Route safety (line-of-sight breaks via obstacles)
                float sf = 1.0f;
                Vector3 toChaser = (chaserPos - pt);
                if (Physics.Raycast(pt + Vector3.up * 1.0f, toChaser.normalized, toChaser.magnitude, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    sf += 2.5f; // Obstacle breaks line of sight from Chaser!
                }

                // 3. E(r): Effectiveness (distance away from current position to maintain momentum)
                float distFromSelf = Vector3.Distance(pt, transform.position);
                float e = Mathf.Clamp(distFromSelf, 1.5f, 5.0f);

                // 4. R(r): Risk factor (boundary proximity penalty)
                float r = 0f;
                float borderDistX = Mathf.Abs(pt.x) - (arenaRadius - 1.2f);
                float borderDistZ = Mathf.Abs(pt.z) - (arenaRadius - 1.2f);
                if (borderDistX > 0f) r += borderDistX * (lateClock ? 4.0f : 2.0f);
                if (borderDistZ > 0f) r += borderDistZ * (lateClock ? 4.0f : 2.0f);

                // If Chaser is between self and the point, heavily penalize
                Vector3 dirToPoint = (pt - transform.position).normalized;
                Vector3 dirToChaser = (chaserPos - transform.position).normalized;
                if (Vector3.Dot(dirToPoint, dirToChaser) > 0.7f && distToChaser < distFromSelf)
                {
                    r += 8.0f; // Running towards the chaser!
                }

                // Calculate total formula score
                float score = (weightDistance * d) + (weightSafety * sf) + (weightEffectiveness * e) - (weightRisk * r);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPoint = pt;
                }
            }

            currentDestination = bestPoint;
        }
    }
}
