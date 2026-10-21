using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.Visuals
{
    /// <summary>
    /// Builds high-visibility stylized athletic 3D visuals for athletes,
    /// complete with team-colored jersey, running gear, and an overhead billboard marker.
    /// </summary>
    public class WCTAthleteVisuals : MonoBehaviour
    {
        [Header("Athlete Info")]
        [SerializeField] private string athleteName = "OPPONENT";
        [SerializeField] private ChaseRole currentRole = ChaseRole.Evader;

        private GameObject visualsRoot;
        private MeshRenderer jerseyRenderer;
        private Transform billboardTag;
        private TextMesh roleTextMesh;
        private TextMesh distTextMesh;
        private Camera mainCamera;

        private Transform leftArmTr;
        private Transform rightArmTr;
        private Transform leftLegTr;
        private Transform rightLegTr;
        private Transform leftCalfTr;
        private Transform rightCalfTr;
        private Transform torsoTr;
        private Vector3 lastPos;
        private float runCycle = 0f;

        public string AthleteName
        {
            get => athleteName;
            set
            {
                athleteName = value;
                UpdateOverheadDisplay();
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            lastPos = transform.position;
            BuildAthleteBody();
            BuildOverheadBillboard();
            ApplyRoleColors(currentRole);
        }

        public void SetRole(ChaseRole role, string name = "")
        {
            currentRole = role;
            if (!string.IsNullOrEmpty(name))
            {
                athleteName = name;
            }
            ApplyRoleColors(role);
            UpdateOverheadDisplay();
        }

        private void BuildAthleteBody()
        {
            if (visualsRoot != null) DestroyImmediate(visualsRoot);

            visualsRoot = new GameObject("Athlete_3D_Model");
            visualsRoot.transform.parent = transform;
            visualsRoot.transform.localPosition = Vector3.zero;
            visualsRoot.transform.localRotation = Quaternion.identity;

            Material skinMat = WCTMaterialPalette.GetRealisticSkinMaterial();
            Material darkTightsMat = WCTMaterialPalette.GetPaddedVinylMaterial();
            Material sockMat = WCTMaterialPalette.GetWhitePadMaterial();
            Material shoeMat = WCTMaterialPalette.GetBlackSteelPipeMaterial();
            Material hairMat = WCTMaterialPalette.GetTrussMaterial();

            // 1. Torso Root (Allows forward running lean)
            GameObject torsoObj = new GameObject("Athlete_Torso_Root");
            torsoObj.transform.parent = visualsRoot.transform;
            torsoObj.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            torsoTr = torsoObj.transform;

            // Torso Jersey with Omega Ω printed on back
            GameObject torsoMesh = CreateBodyPart("Jersey_Body", torsoTr, new Vector3(0f, 0.22f, 0f), new Vector3(0.42f, 0.52f, 0.26f), Quaternion.identity, null);
            jerseyRenderer = torsoMesh.GetComponent<MeshRenderer>();

            // Upper Chest / Shoulders taper
            CreateBodyPart("Shoulders", torsoTr, new Vector3(0f, 0.44f, 0f), new Vector3(0.48f, 0.16f, 0.28f), Quaternion.identity, null);

            // 2. Neck and Head
            CreateBodyPart("Neck", torsoTr, new Vector3(0f, 0.56f, 0f), new Vector3(0.14f, 0.12f, 0.14f), Quaternion.identity, skinMat);
            GameObject head = CreateBodyPart("Head", torsoTr, new Vector3(0f, 0.72f, 0.02f), new Vector3(0.22f, 0.26f, 0.22f), Quaternion.identity, skinMat);
            // Athletic Cropped Hair
            CreateBodyPart("Hair", head.transform, new Vector3(0f, 0.08f, -0.02f), new Vector3(0.23f, 0.16f, 0.24f), Quaternion.identity, hairMat);

            // 3. Articulated Arms
            // Left Arm
            GameObject lArmObj = new GameObject("Arm_L_Root");
            lArmObj.transform.parent = torsoTr;
            lArmObj.transform.localPosition = new Vector3(-0.28f, 0.42f, 0f);
            leftArmTr = lArmObj.transform;
            CreateBodyPart("Bicep_L", leftArmTr, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.28f, 0.11f), Quaternion.identity, skinMat);
            CreateBodyPart("Forearm_L", leftArmTr, new Vector3(0f, -0.38f, 0.06f), new Vector3(0.095f, 0.24f, 0.095f), Quaternion.Euler(25f, 0f, 0f), skinMat);
            CreateBodyPart("Hand_L", leftArmTr, new Vector3(0f, -0.52f, 0.12f), new Vector3(0.08f, 0.10f, 0.06f), Quaternion.Euler(30f, 0f, 0f), skinMat);

            // Right Arm
            GameObject rArmObj = new GameObject("Arm_R_Root");
            rArmObj.transform.parent = torsoTr;
            rArmObj.transform.localPosition = new Vector3(0.28f, 0.42f, 0f);
            rightArmTr = rArmObj.transform;
            CreateBodyPart("Bicep_R", rightArmTr, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.28f, 0.11f), Quaternion.identity, skinMat);
            CreateBodyPart("Forearm_R", rightArmTr, new Vector3(0f, -0.38f, 0.06f), new Vector3(0.095f, 0.24f, 0.095f), Quaternion.Euler(25f, 0f, 0f), skinMat);
            CreateBodyPart("Hand_R", rightArmTr, new Vector3(0f, -0.52f, 0.12f), new Vector3(0.08f, 0.10f, 0.06f), Quaternion.Euler(30f, 0f, 0f), skinMat);

            // 4. Athletic Compression Tights (Waist & Pelvis)
            CreateBodyPart("Pelvis", visualsRoot.transform, new Vector3(0f, 0.88f, 0f), new Vector3(0.38f, 0.18f, 0.24f), Quaternion.identity, darkTightsMat);

            // 5. Articulated Legs (matching screenshot: black compression pants, white ankle socks, black/white sneakers)
            // Left Leg
            GameObject lLegObj = new GameObject("Leg_L_Root");
            lLegObj.transform.parent = visualsRoot.transform;
            lLegObj.transform.localPosition = new Vector3(-0.13f, 0.82f, 0f);
            leftLegTr = lLegObj.transform;

            CreateBodyPart("Thigh_L", leftLegTr, new Vector3(0f, -0.22f, 0f), new Vector3(0.15f, 0.42f, 0.15f), Quaternion.identity, darkTightsMat);

            GameObject lCalfObj = new GameObject("Calf_L_Root");
            lCalfObj.transform.parent = leftLegTr;
            lCalfObj.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            leftCalfTr = lCalfObj.transform;

            CreateBodyPart("Calf_L", leftCalfTr, new Vector3(0f, -0.18f, 0f), new Vector3(0.12f, 0.36f, 0.12f), Quaternion.identity, darkTightsMat);
            // White Ankle Sock (Visible in screenshot!)
            CreateBodyPart("Sock_L", leftCalfTr, new Vector3(0f, -0.35f, 0f), new Vector3(0.125f, 0.10f, 0.125f), Quaternion.identity, sockMat);
            // Sneaker
            CreateBodyPart("Shoe_L", leftCalfTr, new Vector3(0f, -0.40f, 0.06f), new Vector3(0.13f, 0.12f, 0.26f), Quaternion.identity, shoeMat);
            CreateBodyPart("ShoeSole_L", leftCalfTr, new Vector3(0f, -0.45f, 0.06f), new Vector3(0.135f, 0.035f, 0.27f), Quaternion.identity, sockMat);

            // Right Leg
            GameObject rLegObj = new GameObject("Leg_R_Root");
            rLegObj.transform.parent = visualsRoot.transform;
            rLegObj.transform.localPosition = new Vector3(0.13f, 0.82f, 0f);
            rightLegTr = rLegObj.transform;

            CreateBodyPart("Thigh_R", rightLegTr, new Vector3(0f, -0.22f, 0f), new Vector3(0.15f, 0.42f, 0.15f), Quaternion.identity, darkTightsMat);

            GameObject rCalfObj = new GameObject("Calf_R_Root");
            rCalfObj.transform.parent = rightLegTr;
            rCalfObj.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            rightCalfTr = rCalfObj.transform;

            CreateBodyPart("Calf_R", rightCalfTr, new Vector3(0f, -0.18f, 0f), new Vector3(0.12f, 0.36f, 0.12f), Quaternion.identity, darkTightsMat);
            // White Ankle Sock
            CreateBodyPart("Sock_R", rightCalfTr, new Vector3(0f, -0.35f, 0f), new Vector3(0.125f, 0.10f, 0.125f), Quaternion.identity, sockMat);
            // Sneaker
            CreateBodyPart("Shoe_R", rightCalfTr, new Vector3(0f, -0.40f, 0.06f), new Vector3(0.13f, 0.12f, 0.26f), Quaternion.identity, shoeMat);
            CreateBodyPart("ShoeSole_R", rightCalfTr, new Vector3(0f, -0.45f, 0.06f), new Vector3(0.135f, 0.035f, 0.27f), Quaternion.identity, sockMat);
        }

        private void BuildOverheadBillboard()
        {
            GameObject billboard = new GameObject("Overhead_Billboard");
            billboard.transform.parent = transform;
            billboard.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            billboardTag = billboard.transform;

            // Role & Name text
            GameObject roleObj = new GameObject("Role_Text");
            roleObj.transform.parent = billboard.transform;
            roleObj.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            roleTextMesh = roleObj.AddComponent<TextMesh>();
            roleTextMesh.fontSize = 32;
            roleTextMesh.characterSize = 0.065f;
            roleTextMesh.anchor = TextAnchor.MiddleCenter;
            roleTextMesh.alignment = TextAlignment.Center;
            roleTextMesh.fontStyle = FontStyle.Bold;

            // Distance text
            GameObject distObj = new GameObject("Dist_Text");
            distObj.transform.parent = billboard.transform;
            distObj.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            distTextMesh = distObj.AddComponent<TextMesh>();
            distTextMesh.fontSize = 24;
            distTextMesh.characterSize = 0.055f;
            distTextMesh.anchor = TextAnchor.MiddleCenter;
            distTextMesh.alignment = TextAlignment.Center;
            distTextMesh.color = Color.white;

            UpdateOverheadDisplay();
        }

        private void Update()
        {
            // Billboard facing camera
            if (billboardTag != null)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    billboardTag.rotation = Quaternion.LookRotation(billboardTag.position - mainCamera.transform.position);

                    float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
                    if (distTextMesh != null)
                    {
                        distTextMesh.text = $"{distance:0.0}m";
                    }
                }
            }

            // Procedural Athletic Running Animation
            Vector3 velocity = (transform.position - lastPos) / Mathf.Max(Time.deltaTime, 0.001f);
            lastPos = transform.position;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            if (speed > 0.5f)
            {
                runCycle += Time.deltaTime * speed * 3.2f;

                // Forward lean into sprint
                float leanAngle = Mathf.Clamp(speed * 3.5f, 8f, 24f);
                if (torsoTr != null)
                {
                    torsoTr.localRotation = Quaternion.Euler(leanAngle, 0f, 0f);
                }

                // Leg stride cycle
                float legStride = Mathf.Sin(runCycle) * 38f;
                if (leftLegTr != null) leftLegTr.localRotation = Quaternion.Euler(legStride, 0f, 0f);
                if (rightLegTr != null) rightLegTr.localRotation = Quaternion.Euler(-legStride, 0f, 0f);

                // Knee flexion
                float leftKnee = (legStride < 0f) ? Mathf.Abs(legStride) * 1.2f : 12f;
                float rightKnee = (-legStride < 0f) ? Mathf.Abs(-legStride) * 1.2f : 12f;
                if (leftCalfTr != null) leftCalfTr.localRotation = Quaternion.Euler(leftKnee, 0f, 0f);
                if (rightCalfTr != null) rightCalfTr.localRotation = Quaternion.Euler(rightKnee, 0f, 0f);

                // Arm swing cycle (Opposing leg motion)
                float armSwing = -Mathf.Sin(runCycle) * 45f;
                if (leftArmTr != null) leftArmTr.localRotation = Quaternion.Euler(armSwing, 0f, 0f);
                if (rightArmTr != null) rightArmTr.localRotation = Quaternion.Euler(-armSwing, 0f, 0f);
            }
            else
            {
                // Idle athletic stance
                if (torsoTr != null) torsoTr.localRotation = Quaternion.Euler(4f, 0f, 0f);
                if (leftLegTr != null) leftLegTr.localRotation = Quaternion.Euler(5f, 0f, 0f);
                if (rightLegTr != null) rightLegTr.localRotation = Quaternion.Euler(-5f, 0f, 0f);
                if (leftCalfTr != null) leftCalfTr.localRotation = Quaternion.identity;
                if (rightCalfTr != null) rightCalfTr.localRotation = Quaternion.identity;
                if (leftArmTr != null) leftArmTr.localRotation = Quaternion.Euler(15f, 0f, -8f);
                if (rightArmTr != null) rightArmTr.localRotation = Quaternion.Euler(15f, 0f, 8f);
            }
        }

        public void ApplyRoleColors(ChaseRole role)
        {
            currentRole = role;
            Material jerseyMat = (role == ChaseRole.Chaser)
                ? WCTMaterialPalette.GetOmegaJerseyMaterial()
                : WCTMaterialPalette.GetEvaderJerseyMaterial();

            if (jerseyRenderer != null && jerseyMat != null)
            {
                jerseyRenderer.material = jerseyMat;
            }

            UpdateOverheadDisplay();
        }

        private void UpdateOverheadDisplay()
        {
            if (roleTextMesh == null) return;

            if (currentRole == ChaseRole.Chaser)
            {
                roleTextMesh.text = $"[ CHASER : {athleteName} ]";
                roleTextMesh.color = new Color(1.0f, 0.45f, 0.1f);
            }
            else
            {
                roleTextMesh.text = $"[ EVADER : {athleteName} ]";
                roleTextMesh.color = new Color(0.15f, 0.85f, 1.0f);
            }
        }

        private GameObject CreateBodyPart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.parent = parent;
            part.transform.localPosition = localPos;
            part.transform.localRotation = localRot;
            part.transform.localScale = localScale;

            Collider col = part.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            MeshRenderer mr = part.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
            }

            return part;
        }
    }
}
