using System.Collections;
using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Movement;

namespace WorldChaseTag.Visuals
{
    /// <summary>
    /// Procedural First-Person Perspective (FPP) animated athletic arms and hands.
    /// Provides running arm swings, vault/mantle hand placements, slide bracing, and tag reach strikes.
    /// </summary>
    public class WCTFPPArms : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WCTParkourController parkourController;

        // Arm hierarchy roots
        private Transform armsRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForearm;
        private Transform rightForearm;
        private Transform leftHand;
        private Transform rightHand;

        private Material leftGloveMat;
        private Material rightGloveMat;

        // Default rest poses
        private Vector3 leftArmDefaultPos = new Vector3(-0.28f, -0.22f, 0.42f);
        private Vector3 rightArmDefaultPos = new Vector3(0.28f, -0.22f, 0.42f);
        private Quaternion leftArmDefaultRot = Quaternion.Euler(15f, 10f, -5f);
        private Quaternion rightArmDefaultRot = Quaternion.Euler(15f, -10f, 5f);

        // Animation states
        private float animCycle = 0f;
        private bool isPerformingAction = false;
        private Coroutine activeActionRoutine;

        private void Start()
        {
            if (parkourController == null)
            {
                parkourController = GetComponentInParent<WCTParkourController>();
            }

            BuildProceduralArms();
            UpdateRoleVisuals(parkourController != null ? parkourController.CurrentRole : ChaseRole.Chaser);

            if (parkourController != null)
            {
                parkourController.OnParkourActionTriggered += HandleParkourAction;
            }
        }

        private void OnDestroy()
        {
            if (parkourController != null)
            {
                parkourController.OnParkourActionTriggered -= HandleParkourAction;
            }
        }

        public void UpdateRoleVisuals(ChaseRole role)
        {
            Color accent = role == ChaseRole.Chaser ? new Color(0.95f, 0.40f, 0.10f) : new Color(0.10f, 0.70f, 0.95f);
            if (leftGloveMat != null) leftGloveMat.color = accent;
            if (rightGloveMat != null) rightGloveMat.color = accent;
        }

        private void BuildProceduralArms()
        {
            // Create root container attached to camera
            GameObject rootObj = new GameObject("FPP_Athletic_Arms");
            armsRoot = rootObj.transform;
            armsRoot.parent = transform;
            armsRoot.localPosition = Vector3.zero;
            armsRoot.localRotation = Quaternion.identity;

            Color accent = new Color(0.95f, 0.40f, 0.10f);
            leftGloveMat = WCTMaterialPalette.GetGloveMaterial(accent);
            rightGloveMat = WCTMaterialPalette.GetGloveMaterial(accent);
            Material skinMat = WCTMaterialPalette.GetSkinMaterial();

            // 1. Build Left Arm
            GameObject leftObj = new GameObject("Arm_Left");
            leftArm = leftObj.transform;
            leftArm.parent = armsRoot;
            leftArm.localPosition = leftArmDefaultPos;
            leftArm.localRotation = leftArmDefaultRot;

            CreateArmLimb(leftArm, "Left_Bicep", new Vector3(0f, 0.05f, -0.08f), new Vector3(0.08f, 0.16f, 0.08f), skinMat);
            
            GameObject leftFA = new GameObject("Forearm_Left");
            leftForearm = leftFA.transform;
            leftForearm.parent = leftArm;
            leftForearm.localPosition = new Vector3(0f, -0.05f, 0.08f);
            leftForearm.localRotation = Quaternion.Euler(-25f, -12f, 0f);
            CreateArmLimb(leftForearm, "Left_Forearm", new Vector3(0f, 0f, 0.08f), new Vector3(0.075f, 0.075f, 0.18f), skinMat);

            // Wristband
            CreateArmLimb(leftForearm, "Left_Wristband", new Vector3(0f, 0f, 0.16f), new Vector3(0.082f, 0.082f, 0.035f), leftGloveMat);

            // Hand
            GameObject leftH = new GameObject("Hand_Left");
            leftHand = leftH.transform;
            leftHand.parent = leftForearm;
            leftHand.localPosition = new Vector3(0f, 0f, 0.20f);
            leftHand.localRotation = Quaternion.Euler(10f, -15f, 0f);
            CreateArmLimb(leftHand, "Left_Palm", new Vector3(0f, 0f, 0.04f), new Vector3(0.08f, 0.04f, 0.08f), leftGloveMat);
            CreateArmLimb(leftHand, "Left_Thumb", new Vector3(0.04f, 0.01f, 0.03f), new Vector3(0.025f, 0.025f, 0.05f), leftGloveMat);
            CreateArmLimb(leftHand, "Left_Fingers", new Vector3(0f, 0f, 0.09f), new Vector3(0.075f, 0.035f, 0.05f), leftGloveMat);

            // 2. Build Right Arm (Mirror)
            GameObject rightObj = new GameObject("Arm_Right");
            rightArm = rightObj.transform;
            rightArm.parent = armsRoot;
            rightArm.localPosition = rightArmDefaultPos;
            rightArm.localRotation = rightArmDefaultRot;

            CreateArmLimb(rightArm, "Right_Bicep", new Vector3(0f, 0.05f, -0.08f), new Vector3(0.08f, 0.16f, 0.08f), skinMat);

            GameObject rightFA = new GameObject("Forearm_Right");
            rightForearm = rightFA.transform;
            rightForearm.parent = rightArm;
            rightForearm.localPosition = new Vector3(0f, -0.05f, 0.08f);
            rightForearm.localRotation = Quaternion.Euler(-25f, 12f, 0f);
            CreateArmLimb(rightForearm, "Right_Forearm", new Vector3(0f, 0f, 0.08f), new Vector3(0.075f, 0.075f, 0.18f), skinMat);

            // Wristband
            CreateArmLimb(rightForearm, "Right_Wristband", new Vector3(0f, 0f, 0.16f), new Vector3(0.082f, 0.082f, 0.035f), rightGloveMat);

            // Hand
            GameObject rightH = new GameObject("Hand_Right");
            rightHand = rightH.transform;
            rightHand.parent = rightForearm;
            rightHand.localPosition = new Vector3(0f, 0f, 0.20f);
            rightHand.localRotation = Quaternion.Euler(10f, 15f, 0f);
            CreateArmLimb(rightHand, "Right_Palm", new Vector3(0f, 0f, 0.04f), new Vector3(0.08f, 0.04f, 0.08f), rightGloveMat);
            CreateArmLimb(rightHand, "Right_Thumb", new Vector3(-0.04f, 0.01f, 0.03f), new Vector3(0.025f, 0.025f, 0.05f), rightGloveMat);
            CreateArmLimb(rightHand, "Right_Fingers", new Vector3(0f, 0f, 0.09f), new Vector3(0.075f, 0.035f, 0.05f), rightGloveMat);
        }

        private GameObject CreateArmLimb(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            limb.name = name;
            limb.transform.parent = parent;
            limb.transform.localPosition = localPos;
            limb.transform.localRotation = Quaternion.identity;
            limb.transform.localScale = localScale;

            // Remove primitive collider to avoid physics interference
            Collider col = limb.GetComponent<Collider>();
            if (col != null) Destroy(col);

            MeshRenderer mr = limb.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return limb;
        }

        private void Update()
        {
            if (isPerformingAction || leftArm == null || rightArm == null) return;

            bool isMoving = false;
            bool isSprinting = false;

            if (parkourController != null)
            {
                isSprinting = parkourController.IsSprinting;
                isMoving = Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f;
            }

            float speed = isSprinting ? 14f : (isMoving ? 8f : 2.5f);
            animCycle += Time.deltaTime * speed;

            float swingAmp = isSprinting ? 0.08f : (isMoving ? 0.04f : 0.008f);
            float rotAmp = isSprinting ? 18f : (isMoving ? 8f : 2f);

            // Natural alternating arm swings
            float leftSwing = Mathf.Sin(animCycle) * swingAmp;
            float rightSwing = -Mathf.Sin(animCycle) * swingAmp;
            float leftRot = Mathf.Sin(animCycle) * rotAmp;
            float rightRot = -Mathf.Sin(animCycle) * rotAmp;

            Vector3 targetLPos = leftArmDefaultPos + new Vector3(0f, leftSwing * 0.5f, leftSwing);
            Vector3 targetRPos = rightArmDefaultPos + new Vector3(0f, rightSwing * 0.5f, rightSwing);
            Quaternion targetLRot = leftArmDefaultRot * Quaternion.Euler(leftRot, 0f, 0f);
            Quaternion targetRRot = rightArmDefaultRot * Quaternion.Euler(rightRot, 0f, 0f);

            leftArm.localPosition = Vector3.Lerp(leftArm.localPosition, targetLPos, Time.deltaTime * 12f);
            rightArm.localPosition = Vector3.Lerp(rightArm.localPosition, targetRPos, Time.deltaTime * 12f);
            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, targetLRot, Time.deltaTime * 12f);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, targetRRot, Time.deltaTime * 12f);
        }

        private void HandleParkourAction(string action)
        {
            if (activeActionRoutine != null)
            {
                StopCoroutine(activeActionRoutine);
            }

            switch (action)
            {
                case "VAULT":
                    activeActionRoutine = StartCoroutine(AnimateVault());
                    break;
                case "MANTLE":
                    activeActionRoutine = StartCoroutine(AnimateMantle());
                    break;
                case "SLIDE":
                    activeActionRoutine = StartCoroutine(AnimateSlide());
                    break;
                case "TAG_ATTEMPT":
                    activeActionRoutine = StartCoroutine(AnimateTagStrike());
                    break;
            }
        }

        private IEnumerator AnimateVault()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.38f;

            Vector3 lStart = leftArm.localPosition;
            Vector3 rStart = rightArm.localPosition;

            // Vault Pose: Both hands thrust forward and down onto hurdle
            Vector3 lVault = new Vector3(-0.16f, -0.32f, 0.58f);
            Vector3 rVault = new Vector3(0.16f, -0.32f, 0.58f);
            Quaternion lRot = Quaternion.Euler(45f, 15f, -20f);
            Quaternion rRot = Quaternion.Euler(45f, -15f, 20f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                leftArm.localPosition = Vector3.Lerp(lStart, lVault, curve);
                rightArm.localPosition = Vector3.Lerp(rStart, rVault, curve);
                leftArm.localRotation = Quaternion.Slerp(leftArmDefaultRot, lRot, curve);
                rightArm.localRotation = Quaternion.Slerp(rightArmDefaultRot, rRot, curve);

                yield return null;
            }

            isPerformingAction = false;
        }

        private IEnumerator AnimateMantle()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.45f;

            // Reach high to ledge then pull down
            Vector3 lHigh = new Vector3(-0.20f, 0.12f, 0.52f);
            Vector3 rHigh = new Vector3(0.20f, 0.12f, 0.52f);
            Vector3 lPull = new Vector3(-0.25f, -0.35f, 0.40f);
            Vector3 rPull = new Vector3(0.25f, -0.35f, 0.40f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (t < 0.4f)
                {
                    float reachT = t / 0.4f;
                    leftArm.localPosition = Vector3.Lerp(leftArmDefaultPos, lHigh, reachT);
                    rightArm.localPosition = Vector3.Lerp(rightArmDefaultPos, rHigh, reachT);
                }
                else
                {
                    float pullT = (t - 0.4f) / 0.6f;
                    leftArm.localPosition = Vector3.Lerp(lHigh, lPull, pullT);
                    rightArm.localPosition = Vector3.Lerp(rHigh, rPull, pullT);
                }

                yield return null;
            }

            isPerformingAction = false;
        }

        private IEnumerator AnimateSlide()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.65f;

            // Slide pose: Left hand low ground brace, right arm high balance
            Vector3 lSlide = new Vector3(-0.35f, -0.42f, 0.35f);
            Vector3 rSlide = new Vector3(0.32f, 0.05f, 0.45f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                leftArm.localPosition = Vector3.Lerp(leftArmDefaultPos, lSlide, curve);
                rightArm.localPosition = Vector3.Lerp(rightArmDefaultPos, rSlide, curve);
                yield return null;
            }

            isPerformingAction = false;
        }

        public IEnumerator AnimateTagStrike()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.28f;

            // Explosive right palm reach forward strike
            Vector3 rStrike = new Vector3(0.08f, -0.06f, 0.72f);
            Quaternion rRot = Quaternion.Euler(10f, -25f, 15f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                rightArm.localPosition = Vector3.Lerp(rightArmDefaultPos, rStrike, curve);
                rightArm.localRotation = Quaternion.Slerp(rightArmDefaultRot, rRot, curve);
                yield return null;
            }

            rightArm.localPosition = rightArmDefaultPos;
            rightArm.localRotation = rightArmDefaultRot;
            isPerformingAction = false;
        }
    }
}
