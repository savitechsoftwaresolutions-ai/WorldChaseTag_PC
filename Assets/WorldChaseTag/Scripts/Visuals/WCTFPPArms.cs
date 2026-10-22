using System.Collections;
using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Movement;

namespace WorldChaseTag.Visuals
{
    /// <summary>
    /// Procedural First-Person Perspective (FPP) Single Athletic Hand and Arm.
    /// Recreates the authentic World Chase Tag broadcast view (matching Panel 7):
    /// Single left hand entering from bottom-left corner with athletic compression sleeve,
    /// digital sports watch, natural palm contours, and 5 articulated fingers.
    /// Leaving the right screen clear for obstacle navigation and tagging opponents.
    /// </summary>
    public class WCTFPPArms : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WCTParkourController parkourController;

        // Single arm hierarchy roots
        private Transform armsRoot;
        private Transform singleArm;
        private Transform forearm;
        private Transform hand;
        private Renderer sleeveRenderer;

        // Natural rest pose (Entering from bottom-left corner matching Panel 7)
        private readonly Vector3 armDefaultPos = new Vector3(-0.24f, -0.19f, 0.44f);
        private readonly Quaternion armDefaultRot = Quaternion.Euler(14f, 16f, -10f);

        // Animation states
        private float animCycle = 0f;
        private bool isPerformingAction = false;
        private Coroutine activeActionRoutine;

        private void Awake()
        {
            // Enforce strict single-instance per camera
            CleanExistingArmObjects();
        }

        private void Start()
        {
            if (parkourController == null)
            {
                parkourController = GetComponentInParent<WCTParkourController>();
            }

            CleanExistingArmObjects();
            BuildSingleAthleticArm();

            UpdateRoleVisuals(parkourController != null ? parkourController.CurrentRole : ChaseRole.Chaser);

            if (parkourController != null)
            {
                parkourController.OnParkourActionTriggered += HandleParkourAction;
                SetArmsVisible(!parkourController.IsThirdPerson);
            }
            else
            {
                SetArmsVisible(true);
            }
        }

        private void OnDestroy()
        {
            if (parkourController != null)
            {
                parkourController.OnParkourActionTriggered -= HandleParkourAction;
            }
        }

        private void CleanExistingArmObjects()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.Contains("Arm") || child.name.Contains("FPP") || child.name.Contains("Hand"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        public void SetArmsVisible(bool visible)
        {
            if (armsRoot != null)
            {
                armsRoot.gameObject.SetActive(visible);
            }
        }

        public void UpdateRoleVisuals(ChaseRole role)
        {
            if (sleeveRenderer != null)
            {
                sleeveRenderer.material = WCTMaterialPalette.GetSleeveMaterial(role);
            }
        }

        private void BuildSingleAthleticArm()
        {
            // Create root container attached to camera
            GameObject rootObj = new GameObject("FPP_Athletic_SingleHand");
            armsRoot = rootObj.transform;
            armsRoot.parent = transform;
            armsRoot.localPosition = Vector3.zero;
            armsRoot.localRotation = Quaternion.identity;

            Material skinMat = WCTMaterialPalette.GetRealisticSkinMaterial();
            Material sleeveMat = WCTMaterialPalette.GetSleeveMaterial(ChaseRole.Chaser);
            Material cuffMat = WCTMaterialPalette.GetWristCuffMaterial();
            Material watchMat = WCTMaterialPalette.GetSportsWatchMaterial();
            Material watchScreenMat = WCTMaterialPalette.GetWatchScreenMaterial();
            Material nailMat = WCTMaterialPalette.GetFingernailMaterial();

            // Single Left Arm (Matching Panel 7 of tournament reference)
            GameObject armObj = new GameObject("Arm_SingleLeft");
            singleArm = armObj.transform;
            singleArm.parent = armsRoot;
            singleArm.localPosition = armDefaultPos;
            singleArm.localRotation = armDefaultRot;

            // 1. Forearm Root with Athletic Compression Sleeve
            GameObject forearmObj = new GameObject("Forearm");
            forearm = forearmObj.transform;
            forearm.parent = singleArm;
            forearm.localPosition = Vector3.zero;
            forearm.localRotation = Quaternion.identity;

            // Tapered compression sleeve: radius 0.038m (elbow) to 0.029m (wrist), length 0.20m
            GameObject sleeveObj = CreateSmoothSegment("Compression_Sleeve", forearm, new Vector3(0f, 0f, 0.09f), Quaternion.identity, 0.038f, 0.029f, 0.18f, sleeveMat);
            sleeveRenderer = sleeveObj.GetComponent<Renderer>();

            // Wrist Compression Cuff (Dark elastic band at wrist)
            CreateSmoothSegment("Wrist_Cuff", forearm, new Vector3(0f, 0f, 0.19f), Quaternion.identity, 0.0295f, 0.0285f, 0.022f, cuffMat);

            // 2. Digital Sports Watch on dorsal wrist (Live telemetry display)
            BuildSportsWatch(forearm, new Vector3(0f, 0.016f, 0.178f), watchMat, watchScreenMat);

            // 3. Wrist & Hand Root
            GameObject handObj = new GameObject("Hand_Anatomical");
            hand = handObj.transform;
            hand.parent = forearm;
            hand.localPosition = new Vector3(0f, 0.002f, 0.215f);
            hand.localRotation = Quaternion.Euler(4f, -8f, 2f);

            // Smooth Anatomical Wrist Transition
            CreateSmoothSegment("Wrist_Base", hand, new Vector3(0f, 0f, 0.012f), Quaternion.identity, 0.028f, 0.033f, 0.025f, skinMat);

            // Palm Body: natural flattened human palm (0.068m wide, 0.025m thick, 0.062m long)
            GameObject palm = CreateSmoothSegment("Palm_Body", hand, new Vector3(0f, -0.002f, 0.045f), Quaternion.identity, 0.033f, 0.036f, 0.052f, skinMat);
            palm.transform.localScale = new Vector3(1.15f, 0.52f, 1.0f);

            // Thenar Muscle Mound (Prominent muscular thumb base on radial side)
            GameObject thenar = CreateSmoothSegment("Thenar_Eminence", hand, new Vector3(-0.024f, -0.006f, 0.032f), Quaternion.Euler(0f, -22f, 0f), 0.018f, 0.014f, 0.034f, skinMat);
            thenar.transform.localScale = new Vector3(1.0f, 0.75f, 1.0f);

            // Hypothenar Cushion (Padding along ulnar edge of palm)
            GameObject hypothenar = CreateSmoothSegment("Hypothenar_Cushion", hand, new Vector3(0.022f, -0.004f, 0.038f), Quaternion.Euler(0f, 10f, 0f), 0.015f, 0.012f, 0.036f, skinMat);
            hypothenar.transform.localScale = new Vector3(0.85f, 0.65f, 1.0f);

            // 4. 5 Naturally Articulated Fingers (Thumb, Index, Middle, Ring, Pinky)
            BuildNaturalFingers(hand, skinMat, nailMat);
        }

        private void BuildSportsWatch(Transform parent, Vector3 localPos, Material watchMat, Material screenMat)
        {
            GameObject watchRoot = new GameObject("DigitalSportsWatch");
            watchRoot.transform.parent = parent;
            watchRoot.transform.localPosition = localPos;
            watchRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Silicone Strap
            GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strap.name = "Watch_Strap";
            strap.transform.parent = watchRoot.transform;
            strap.transform.localPosition = Vector3.zero;
            strap.transform.localRotation = Quaternion.identity;
            strap.transform.localScale = new Vector3(0.064f, 0.014f, 0.064f);
            DestroyImmediate(strap.GetComponent<Collider>());
            ApplyMaterial(strap, watchMat);

            // Beveled Watch Casing
            GameObject caseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            caseObj.name = "Watch_Case";
            caseObj.transform.parent = watchRoot.transform;
            caseObj.transform.localPosition = new Vector3(0f, 0.003f, -0.031f);
            caseObj.transform.localRotation = Quaternion.identity;
            caseObj.transform.localScale = new Vector3(0.036f, 0.014f, 0.032f);
            DestroyImmediate(caseObj.GetComponent<Collider>());
            ApplyMaterial(caseObj, watchMat);

            // LCD Telemetry Display Screen
            GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screenObj.name = "Watch_Screen";
            screenObj.transform.parent = caseObj.transform;
            screenObj.transform.localPosition = new Vector3(0f, 0.015f, -0.52f);
            screenObj.transform.localRotation = Quaternion.identity;
            screenObj.transform.localScale = new Vector3(0.82f, 0.80f, 0.05f);
            DestroyImmediate(screenObj.GetComponent<Collider>());
            ApplyMaterial(screenObj, screenMat);
        }

        private void BuildNaturalFingers(Transform handRoot, Material skinMat, Material nailMat)
        {
            // 1. Thumb (2 phalanges, abducted radially -32° and forward 18°)
            BuildFingerChain(handRoot, "Thumb",
                new Vector3(-0.030f, -0.006f, 0.024f),
                new Vector3(16f, -34f, -14f),
                new float[] { 0.030f, 0.024f },
                new float[] { 0.0135f, 0.012f, 0.0095f },
                new float[] { 14f, 16f },
                skinMat, nailMat, true);

            // 2. Index Finger (3 phalanges, gentle resting athletic parkour curve)
            BuildFingerChain(handRoot, "Index",
                new Vector3(-0.022f, 0.001f, 0.062f),
                new Vector3(10f, -4f, 0f),
                new float[] { 0.034f, 0.022f, 0.016f },
                new float[] { 0.0125f, 0.0115f, 0.010f, 0.008f },
                new float[] { 12f, 14f, 10f },
                skinMat, nailMat, false);

            // 3. Middle Finger (Longest, prominent knuckle)
            BuildFingerChain(handRoot, "Middle",
                new Vector3(-0.006f, 0.003f, 0.066f),
                new Vector3(12f, 0f, 0f),
                new float[] { 0.038f, 0.024f, 0.018f },
                new float[] { 0.013f, 0.012f, 0.0105f, 0.0085f },
                new float[] { 14f, 16f, 12f },
                skinMat, nailMat, false);

            // 4. Ring Finger (Slightly more curled)
            BuildFingerChain(handRoot, "Ring",
                new Vector3(0.009f, 0.002f, 0.063f),
                new Vector3(15f, 5f, 0f),
                new float[] { 0.034f, 0.022f, 0.016f },
                new float[] { 0.0125f, 0.0115f, 0.010f, 0.008f },
                new float[] { 18f, 20f, 14f },
                skinMat, nailMat, false);

            // 5. Pinky Finger (Curled most for natural relaxed hand pose)
            BuildFingerChain(handRoot, "Pinky",
                new Vector3(0.023f, -0.001f, 0.056f),
                new Vector3(18f, 12f, 0f),
                new float[] { 0.026f, 0.017f, 0.013f },
                new float[] { 0.011f, 0.010f, 0.0085f, 0.007f },
                new float[] { 22f, 24f, 16f },
                skinMat, nailMat, false);
        }

        private void BuildFingerChain(Transform parent, string name, Vector3 basePos, Vector3 baseAngles, float[] lengths, float[] radii, float[] curls, Material skinMat, Material nailMat, bool isThumb)
        {
            GameObject chainRoot = new GameObject("Finger_" + name);
            chainRoot.transform.parent = parent;
            chainRoot.transform.localPosition = basePos;
            chainRoot.transform.localRotation = Quaternion.Euler(baseAngles);

            Transform currentJoint = chainRoot.transform;

            for (int i = 0; i < lengths.Length; i++)
            {
                float len = lengths[i];
                float rStart = radii[i];
                float rEnd = radii[i + 1];

                // Smooth tapered phalanx segment
                CreateSmoothSegment("Phalanx_" + i, currentJoint, new Vector3(0f, 0f, len * 0.5f), Quaternion.identity, rStart, rEnd, len, skinMat);

                // Next joint
                if (i < lengths.Length - 1)
                {
                    GameObject nextJointObj = new GameObject("Joint_" + i);
                    nextJointObj.transform.parent = currentJoint;
                    nextJointObj.transform.localPosition = new Vector3(0f, 0f, len);
                    nextJointObj.transform.localRotation = Quaternion.Euler(curls[i], 0f, 0f);
                    currentJoint = nextJointObj.transform;
                }
                else
                {
                    // Distal Fingertip with natural rounded dome and fingernail
                    GameObject tipCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tipCap.name = "Tip_Dome";
                    tipCap.transform.parent = currentJoint;
                    tipCap.transform.localPosition = new Vector3(0f, 0f, len);
                    tipCap.transform.localScale = Vector3.one * (rEnd * 2f);
                    DestroyImmediate(tipCap.GetComponent<Collider>());
                    ApplyMaterial(tipCap, skinMat);

                    // Keratin Fingernail on dorsal side
                    GameObject nail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    nail.name = "Fingernail";
                    nail.transform.parent = currentJoint;
                    nail.transform.localPosition = new Vector3(0f, rEnd * 0.78f, len - rEnd * 0.35f);
                    nail.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
                    nail.transform.localScale = new Vector3(rEnd * 1.35f, 0.0016f, rEnd * 1.6f);
                    DestroyImmediate(nail.GetComponent<Collider>());
                    ApplyMaterial(nail, nailMat);
                }
            }
        }

        private GameObject CreateSmoothSegment(string name, Transform parent, Vector3 localPos, Quaternion localRot, float rStart, float rEnd, float length, Material mat)
        {
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = localRot;

            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            mf.sharedMesh = GenerateTaperedCylinderMesh(rStart, rEnd, length, 14);

            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;

            return obj;
        }

        private Mesh GenerateTaperedCylinderMesh(float rStart, float rEnd, float length, int radialSegments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "SmoothSegmentMesh";

            int vCount = (radialSegments + 1) * 2;
            Vector3[] vertices = new Vector3[vCount];
            Vector3[] normals = new Vector3[vCount];
            Vector2[] uvs = new Vector2[vCount];
            int[] triangles = new int[radialSegments * 6];

            float halfLen = length * 0.5f;

            for (int i = 0; i <= radialSegments; i++)
            {
                float t = (float)i / radialSegments;
                float angle = t * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                // Bottom ring (-halfLen)
                int v0 = i;
                vertices[v0] = new Vector3(cos * rStart, sin * rStart, -halfLen);
                normals[v0] = new Vector3(cos, sin, 0f).normalized;
                uvs[v0] = new Vector2(t, 0f);

                // Top ring (+halfLen)
                int v1 = i + radialSegments + 1;
                vertices[v1] = new Vector3(cos * rEnd, sin * rEnd, halfLen);
                normals[v1] = new Vector3(cos, sin, 0f).normalized;
                uvs[v1] = new Vector2(t, 1f);
            }

            int triIndex = 0;
            for (int i = 0; i < radialSegments; i++)
            {
                int b0 = i;
                int b1 = i + 1;
                int t0 = i + radialSegments + 1;
                int t1 = i + radialSegments + 2;

                triangles[triIndex++] = b0;
                triangles[triIndex++] = t0;
                triangles[triIndex++] = b1;

                triangles[triIndex++] = b1;
                triangles[triIndex++] = t0;
                triangles[triIndex++] = t1;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }

        private void ApplyMaterial(GameObject obj, Material mat)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }
        }

        private void Update()
        {
            if (isPerformingAction || singleArm == null) return;

            bool isMoving = false;
            bool isSprinting = false;

            if (parkourController != null)
            {
                isSprinting = parkourController.IsSprinting;
                isMoving = Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f;
            }

            // Stride cycle frequency
            float speed = isSprinting ? 13f : (isMoving ? 7.5f : 2.2f);
            animCycle += Time.deltaTime * speed;

            float swingAmp = isSprinting ? 0.055f : (isMoving ? 0.028f : 0.005f);
            float rotAmp = isSprinting ? 12f : (isMoving ? 5f : 1.5f);

            // Natural athletic left hand movement
            float swing = Mathf.Sin(animCycle) * swingAmp;
            float rot = Mathf.Sin(animCycle) * rotAmp;
            float sway = Mathf.Cos(animCycle * 0.5f) * (swingAmp * 0.4f);

            Vector3 targetPos = armDefaultPos + new Vector3(sway, swing * 0.5f, swing);
            Quaternion targetRot = armDefaultRot * Quaternion.Euler(rot, rot * 0.4f, 0f);

            singleArm.localPosition = Vector3.Lerp(singleArm.localPosition, targetPos, Time.deltaTime * 12f);
            singleArm.localRotation = Quaternion.Slerp(singleArm.localRotation, targetRot, Time.deltaTime * 12f);
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
            float duration = 0.35f;

            Vector3 startPos = singleArm.localPosition;
            Quaternion startRot = singleArm.localRotation;

            // Vault Pose: Hand pushes firmly down onto hurdle top
            Vector3 vaultPos = new Vector3(-0.14f, -0.28f, 0.54f);
            Quaternion vaultRot = Quaternion.Euler(38f, 18f, -18f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                singleArm.localPosition = Vector3.Lerp(startPos, vaultPos, curve);
                singleArm.localRotation = Quaternion.Slerp(startRot, vaultRot, curve);
                yield return null;
            }

            singleArm.localPosition = armDefaultPos;
            singleArm.localRotation = armDefaultRot;
            isPerformingAction = false;
        }

        private IEnumerator AnimateMantle()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.42f;

            Vector3 highGrip = new Vector3(-0.16f, 0.08f, 0.50f);
            Vector3 pullDown = new Vector3(-0.20f, -0.30f, 0.42f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (t < 0.45f)
                {
                    float reachT = t / 0.45f;
                    singleArm.localPosition = Vector3.Lerp(armDefaultPos, highGrip, reachT);
                }
                else
                {
                    float pullT = (t - 0.45f) / 0.55f;
                    singleArm.localPosition = Vector3.Lerp(highGrip, pullDown, pullT);
                }
                yield return null;
            }

            singleArm.localPosition = armDefaultPos;
            singleArm.localRotation = armDefaultRot;
            isPerformingAction = false;
        }

        private IEnumerator AnimateSlide()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.60f;

            // Slide pose: Low floor brace
            Vector3 slidePos = new Vector3(-0.28f, -0.36f, 0.38f);
            Quaternion slideRot = Quaternion.Euler(42f, 25f, -22f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                singleArm.localPosition = Vector3.Lerp(armDefaultPos, slidePos, curve);
                singleArm.localRotation = Quaternion.Slerp(armDefaultRot, slideRot, curve);
                yield return null;
            }

            singleArm.localPosition = armDefaultPos;
            singleArm.localRotation = armDefaultRot;
            isPerformingAction = false;
        }

        public IEnumerator AnimateTagStrike()
        {
            isPerformingAction = true;
            float elapsed = 0f;
            float duration = 0.26f;

            // Tag strike: Explosive athletic forward reach with open palm towards crosshair!
            Vector3 strikePos = new Vector3(-0.06f, -0.04f, 0.68f);
            Quaternion strikeRot = Quaternion.Euler(6f, 8f, -4f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);

                singleArm.localPosition = Vector3.Lerp(armDefaultPos, strikePos, curve);
                singleArm.localRotation = Quaternion.Slerp(armDefaultRot, strikeRot, curve);
                yield return null;
            }

            singleArm.localPosition = armDefaultPos;
            singleArm.localRotation = armDefaultRot;
            isPerformingAction = false;
        }
    }
}
