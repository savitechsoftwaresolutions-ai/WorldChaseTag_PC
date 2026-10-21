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

            Material skinMat = WCTMaterialPalette.GetRealisticSkinMaterial();
            Material watchMat = WCTMaterialPalette.GetSportsWatchMaterial();
            Material watchScreenMat = WCTMaterialPalette.GetWatchScreenMaterial();
            Material nailMat = WCTMaterialPalette.GetFingernailMaterial();

            // 1. Build Left Arm (with Digital Sports Watch matching screenshot)
            GameObject leftObj = new GameObject("Arm_Left");
            leftArm = leftObj.transform;
            leftArm.parent = armsRoot;
            leftArm.localPosition = leftArmDefaultPos;
            leftArm.localRotation = leftArmDefaultRot;

            BuildAnatomicalArm(leftArm, true, skinMat, watchMat, watchScreenMat, nailMat);

            // 2. Build Right Arm (Extended forward matching screenshot)
            GameObject rightObj = new GameObject("Arm_Right");
            rightArm = rightObj.transform;
            rightArm.parent = armsRoot;
            rightArm.localPosition = rightArmDefaultPos;
            rightArm.localRotation = rightArmDefaultRot;

            BuildAnatomicalArm(rightArm, false, skinMat, watchMat, watchScreenMat, nailMat);
        }

        private void BuildAnatomicalArm(Transform armRoot, bool isLeft, Material skinMat, Material watchMat, Material watchScreenMat, Material nailMat)
        {
            float sideSign = isLeft ? -1f : 1f;

            // Bicep / Upper Arm
            GameObject bicep = CreateSmoothCapsule("Bicep", armRoot, new Vector3(0f, 0.06f, -0.14f), new Vector3(0.095f, 0.16f, 0.095f), Quaternion.Euler(20f, sideSign * 8f, 0f), skinMat);

            // Forearm Root
            GameObject forearmObj = new GameObject(isLeft ? "Forearm_Left" : "Forearm_Right");
            Transform forearm = forearmObj.transform;
            forearm.parent = armRoot;
            forearm.localPosition = new Vector3(0f, -0.04f, 0.04f);
            forearm.localRotation = Quaternion.Euler(-18f, sideSign * -10f, sideSign * 5f);

            if (isLeft) leftForearm = forearm; else rightForearm = forearm;

            // Tapered muscular forearm (Main brachioradialis body)
            CreateSmoothCapsule("Forearm_Muscle", forearm, new Vector3(0f, -0.01f, 0.08f), new Vector3(0.082f, 0.17f, 0.088f), Quaternion.Euler(88f, 0f, 0f), skinMat);
            // Wrist taper
            CreateSmoothCapsule("Wrist_Joint", forearm, new Vector3(0f, 0f, 0.175f), new Vector3(0.068f, 0.06f, 0.068f), Quaternion.Euler(90f, 0f, 0f), skinMat);

            // On Left Arm: Build Digital Sports Watch (Visible in Screenshot!)
            if (isLeft)
            {
                BuildSportsWatch(forearm, new Vector3(0f, 0.018f, 0.165f), watchMat, watchScreenMat);
            }

            // Hand Root
            GameObject handObj = new GameObject(isLeft ? "Hand_Left" : "Hand_Right");
            Transform hand = handObj.transform;
            hand.parent = forearm;
            hand.localPosition = new Vector3(0f, 0.005f, 0.205f);
            hand.localRotation = Quaternion.Euler(8f, sideSign * -14f, sideSign * 4f);

            if (isLeft) leftHand = hand; else rightHand = hand;

            // Anatomical Palm Body
            CreateSmoothCapsule("Palm_Center", hand, new Vector3(0f, -0.004f, 0.042f), new Vector3(0.084f, 0.034f, 0.082f), Quaternion.identity, skinMat);
            // Thenar muscle mound (thumb base)
            CreateSmoothCapsule("Palm_Thenar", hand, new Vector3(sideSign * 0.030f, -0.008f, 0.024f), new Vector3(0.038f, 0.030f, 0.046f), Quaternion.Euler(0f, sideSign * 25f, 0f), skinMat);

            // Build 5 Articulated Fingers matching Screenshot pose
            BuildAllFingers(hand, isLeft, sideSign, skinMat, nailMat);
        }

        private void BuildSportsWatch(Transform parent, Vector3 localPos, Material watchMat, Material screenMat)
        {
            GameObject watchRoot = new GameObject("DigitalSportsWatch");
            watchRoot.transform.parent = parent;
            watchRoot.transform.localPosition = localPos;
            watchRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // 1. Silicone Watch Strap encircling the wrist
            GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strap.name = "Watch_Strap";
            strap.transform.parent = watchRoot.transform;
            strap.transform.localPosition = Vector3.zero;
            strap.transform.localRotation = Quaternion.identity;
            strap.transform.localScale = new Vector3(0.076f, 0.016f, 0.076f);
            DestroyImmediate(strap.GetComponent<Collider>());
            ApplyMaterial(strap, watchMat);

            // 2. Beveled Watch Casing
            GameObject caseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            caseObj.name = "Watch_Case";
            caseObj.transform.parent = watchRoot.transform;
            caseObj.transform.localPosition = new Vector3(0f, 0.004f, -0.036f);
            caseObj.transform.localRotation = Quaternion.identity;
            caseObj.transform.localScale = new Vector3(0.042f, 0.018f, 0.038f);
            DestroyImmediate(caseObj.GetComponent<Collider>());
            ApplyMaterial(caseObj, watchMat);

            // 3. LCD Digital Chrono Screen
            GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screenObj.name = "Watch_Screen";
            screenObj.transform.parent = caseObj.transform;
            screenObj.transform.localPosition = new Vector3(0f, 0.02f, -0.52f);
            screenObj.transform.localRotation = Quaternion.identity;
            screenObj.transform.localScale = new Vector3(0.84f, 0.82f, 0.05f);
            DestroyImmediate(screenObj.GetComponent<Collider>());
            ApplyMaterial(screenObj, screenMat);

            // 4. Metallic Side Pushers
            GameObject button1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            button1.name = "Watch_Pusher";
            button1.transform.parent = caseObj.transform;
            button1.transform.localPosition = new Vector3(0.55f, 0f, -0.1f);
            button1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            button1.transform.localScale = new Vector3(0.18f, 0.12f, 0.18f);
            DestroyImmediate(button1.GetComponent<Collider>());
            ApplyMaterial(button1, watchMat);
        }

        private void BuildAllFingers(Transform hand, bool isLeft, float sideSign, Material skinMat, Material nailMat)
        {
            // Natural resting hand curvature matching Screenshot
            // Thumb
            BuildArticulatedFinger(hand, "Finger_Thumb",
                new Vector3(sideSign * 0.042f, -0.010f, 0.024f),
                new Vector3(sideSign * 38f, sideSign * 32f, sideSign * -22f),
                0.058f, 0.019f, 22f, skinMat, nailMat);

            // Index Finger
            BuildArticulatedFinger(hand, "Finger_Index",
                new Vector3(sideSign * 0.028f, 0.002f, 0.076f),
                new Vector3(24f, sideSign * 6f, 0f),
                0.078f, 0.016f, 28f, skinMat, nailMat);

            // Middle Finger (Longest)
            BuildArticulatedFinger(hand, "Finger_Middle",
                new Vector3(sideSign * 0.008f, 0.004f, 0.082f),
                new Vector3(26f, 0f, 0f),
                0.086f, 0.017f, 32f, skinMat, nailMat);

            // Ring Finger
            BuildArticulatedFinger(hand, "Finger_Ring",
                new Vector3(sideSign * -0.014f, 0.002f, 0.078f),
                new Vector3(28f, sideSign * -6f, 0f),
                0.080f, 0.016f, 35f, skinMat, nailMat);

            // Little Finger (Pinky)
            BuildArticulatedFinger(hand, "Finger_Pinky",
                new Vector3(sideSign * -0.034f, -0.002f, 0.070f),
                new Vector3(32f, sideSign * -12f, 0f),
                0.065f, 0.014f, 40f, skinMat, nailMat);
        }

        private void BuildArticulatedFinger(Transform parent, string name, Vector3 basePos, Vector3 baseAngles, float length, float thickness, float curlAngle, Material skinMat, Material nailMat)
        {
            GameObject fingerRoot = new GameObject(name);
            fingerRoot.transform.parent = parent;
            fingerRoot.transform.localPosition = basePos;
            fingerRoot.transform.localRotation = Quaternion.Euler(baseAngles);

            float p1Len = length * 0.44f;
            float p2Len = length * 0.32f;
            float p3Len = length * 0.24f;

            // Proximal Phalanx
            GameObject p1 = CreateSmoothCapsule("Phalanx_Proximal", fingerRoot.transform, new Vector3(0f, 0f, p1Len * 0.5f), new Vector3(thickness, thickness * 0.88f, p1Len), Quaternion.identity, skinMat);

            // Knuckle Joint
            GameObject knuckle1 = new GameObject("Joint_Inter1");
            knuckle1.transform.parent = fingerRoot.transform;
            knuckle1.transform.localPosition = new Vector3(0f, 0f, p1Len);
            knuckle1.transform.localRotation = Quaternion.Euler(curlAngle, 0f, 0f);

            // Middle Phalanx
            GameObject p2 = CreateSmoothCapsule("Phalanx_Middle", knuckle1.transform, new Vector3(0f, 0f, p2Len * 0.5f), new Vector3(thickness * 0.90f, thickness * 0.82f, p2Len), Quaternion.identity, skinMat);

            // Distal Joint
            GameObject knuckle2 = new GameObject("Joint_Distal");
            knuckle2.transform.parent = knuckle1.transform;
            knuckle2.transform.localPosition = new Vector3(0f, 0f, p2Len);
            knuckle2.transform.localRotation = Quaternion.Euler(curlAngle * 0.75f, 0f, 0f);

            // Distal Phalanx (Fingertip)
            GameObject p3 = CreateSmoothCapsule("Phalanx_Distal", knuckle2.transform, new Vector3(0f, 0f, p3Len * 0.5f), new Vector3(thickness * 0.82f, thickness * 0.75f, p3Len), Quaternion.identity, skinMat);

            // Fingernail plate on dorsal surface
            GameObject nail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nail.name = "Fingernail";
            nail.transform.parent = p3.transform;
            nail.transform.localPosition = new Vector3(0f, 0.46f, 0.15f);
            nail.transform.localRotation = Quaternion.identity;
            nail.transform.localScale = new Vector3(0.68f, 0.08f, 0.55f);
            DestroyImmediate(nail.GetComponent<Collider>());
            ApplyMaterial(nail, nailMat);
        }

        private GameObject CreateSmoothCapsule(string name, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = name;
            obj.transform.parent = parent;
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = localRot;
            obj.transform.localScale = localScale;

            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }

            return obj;
        }

        private void ApplyMaterial(GameObject obj, Material mat)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
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
