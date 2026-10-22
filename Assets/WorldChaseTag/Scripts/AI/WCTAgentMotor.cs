using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace WorldChaseTag.AI
{
    [RequireComponent(typeof(CharacterController))]
    public class WCTAgentMotor : MonoBehaviour
    {
        [Header("Movement (Configurable Athletic Targets)")]
        [SerializeField] private float walkSpeed = 2.4f;
        [SerializeField] private float sprintSpeed = 5.6f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float turnSpeed = 9f;
        [SerializeField] private float gravity = -20f;

        [Header("Parkour & Obstacles")]
        [SerializeField] private LayerMask obstacleLayer = ~0;
        [SerializeField] private float vaultDuration = 0.38f;
        [SerializeField] private float mantleDuration = 0.45f;

        private CharacterController controller;
        private NavMeshAgent navMeshAgent;
        private Vector3 currentVelocity;
        private float verticalVelocity;
        private bool isPerformingParkour = false;

        public bool IsPerformingParkour => isPerformingParkour;
        public float CurrentSpeed => new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        public void ApplyDifficultySettings(WorldChaseTag.Core.AIDifficulty diff)
        {
            switch (diff)
            {
                case WorldChaseTag.Core.AIDifficulty.Easy:
                    walkSpeed = 2.0f;
                    sprintSpeed = 4.8f;
                    acceleration = 9f;
                    turnSpeed = 6f;
                    break;
                case WorldChaseTag.Core.AIDifficulty.Normal:
                    walkSpeed = 2.4f;
                    sprintSpeed = 5.6f;
                    acceleration = 12f;
                    turnSpeed = 9f;
                    break;
                case WorldChaseTag.Core.AIDifficulty.Hard:
                    walkSpeed = 2.4f;
                    sprintSpeed = 6.1f;
                    acceleration = 14f;
                    turnSpeed = 12f;
                    break;
                case WorldChaseTag.Core.AIDifficulty.Expert:
                    walkSpeed = 2.5f;
                    sprintSpeed = 6.2f;
                    acceleration = 16f;
                    turnSpeed = 14f;
                    break;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            if (navMeshAgent != null)
            {
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
            }
        }

        public void MoveToward(Vector3 targetPosition, bool sprint)
        {
            if (isPerformingParkour) return;

            Vector3 diff = targetPosition - transform.position;
            diff.y = 0f;

            if (diff.magnitude < 0.2f)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
            }
            else
            {
                Vector3 desiredDir = diff.normalized;

                // Rotate towards movement direction
                Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

                float targetSpeed = sprint ? sprintSpeed : walkSpeed;
                Vector3 targetVel = desiredDir * targetSpeed;
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVel, acceleration * Time.deltaTime);

                // Check for obstacle in path
                CheckForwardObstacle();
            }

            // Gravity
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2.5f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 finalMove = currentVelocity + Vector3.up * verticalVelocity;
            controller.Move(finalMove * Time.deltaTime);

            // Wall slide resolution: prevent AI from getting stuck against walls
            if ((controller.collisionFlags & CollisionFlags.Sides) != 0 && currentVelocity.sqrMagnitude > 0.01f)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.6f, currentVelocity.normalized, out RaycastHit wallGlanceHit, 0.65f, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, wallGlanceHit.normal);
                }
            }

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.nextPosition = transform.position;
            }
        }

        private void CheckForwardObstacle()
        {
            if (isPerformingParkour) return;

            Vector3 forward = transform.forward;
            RaycastHit wallHit;
            bool hitWall = Physics.Raycast(transform.position + Vector3.up * 0.55f, forward, out wallHit, 1.4f, obstacleLayer, QueryTriggerInteraction.Ignore) ||
                           Physics.Raycast(transform.position + Vector3.up * 0.25f, forward, out wallHit, 1.4f, obstacleLayer, QueryTriggerInteraction.Ignore);

            if (hitWall)
            {
                float[] sampleOffsets = new float[] { 0.12f, 0.28f, 0.45f };
                RaycastHit ledgeHit = default;
                bool foundLedge = false;

                foreach (float offset in sampleOffsets)
                {
                    Vector3 testTopOrigin = wallHit.point + forward * offset + Vector3.up * 2.5f;
                    if (Physics.Raycast(testTopOrigin, Vector3.down, out RaycastHit candidateHit, 3.0f, obstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        float h = candidateHit.point.y - transform.position.y;
                        if (h >= 0.45f && h <= 2.2f)
                        {
                            ledgeHit = candidateHit;
                            foundLedge = true;
                            break;
                        }
                    }
                }

                if (foundLedge)
                {
                    float obstacleHeight = ledgeHit.point.y - transform.position.y;
                    if (obstacleHeight >= 0.45f && obstacleHeight <= 1.25f)
                    {
                        // Low obstacle vault
                        Vector3 targetLanding = wallHit.point + forward * 2.2f;
                        targetLanding.y = transform.position.y;
                        StartCoroutine(ExecuteAIVault(ledgeHit.point, targetLanding));
                    }
                    else if (obstacleHeight > 1.25f && obstacleHeight <= 2.2f)
                    {
                        // Mantle
                        Vector3 targetLedgeTop = ledgeHit.point + forward * 0.3f + Vector3.up * 0.05f;
                        StartCoroutine(ExecuteAIMantle(targetLedgeTop));
                    }
                }
            }
        }

        private IEnumerator ExecuteAIVault(Vector3 peakPoint, Vector3 landingPoint)
        {
            isPerformingParkour = true;
            controller.enabled = false;

            Vector3 startPos = transform.position;
            Vector3 peak = peakPoint + Vector3.up * 0.4f;

            float elapsed = 0f;
            while (elapsed < vaultDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / vaultDuration);

                Vector3 m1 = Vector3.Lerp(startPos, peak, t);
                Vector3 m2 = Vector3.Lerp(peak, landingPoint, t);
                transform.position = Vector3.Lerp(m1, m2, t);

                yield return null;
            }

            transform.position = landingPoint;
            controller.enabled = true;
            verticalVelocity = 0f;
            isPerformingParkour = false;
        }

        private IEnumerator ExecuteAIMantle(Vector3 ledgeTop)
        {
            isPerformingParkour = true;
            controller.enabled = false;

            Vector3 startPos = transform.position;
            Vector3 pullUpPos = new Vector3(startPos.x, ledgeTop.y + 0.2f, startPos.z);

            float elapsed = 0f;
            while (elapsed < mantleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mantleDuration);

                if (t < 0.6f)
                {
                    float pullT = t / 0.6f;
                    transform.position = Vector3.Lerp(startPos, pullUpPos, pullT);
                }
                else
                {
                    float stepT = (t - 0.6f) / 0.4f;
                    transform.position = Vector3.Lerp(pullUpPos, ledgeTop, stepT);
                }

                yield return null;
            }

            transform.position = ledgeTop;
            controller.enabled = true;
            verticalVelocity = 0f;
            isPerformingParkour = false;
        }

        public void TeleportTo(Vector3 pos, Quaternion rot)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = pos;
            transform.rotation = rot;
            currentVelocity = Vector3.zero;
            verticalVelocity = 0f;
            controller.enabled = wasEnabled;

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.Warp(pos);
            }
        }
    }
}
