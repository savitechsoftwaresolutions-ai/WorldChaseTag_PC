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
            if (visualsRoot != null) Destroy(visualsRoot);

            visualsRoot = new GameObject("Athlete_3D_Model");
            visualsRoot.transform.parent = transform;
            visualsRoot.transform.localPosition = Vector3.zero;
            visualsRoot.transform.localRotation = Quaternion.identity;

            Material skinMat = WCTMaterialPalette.GetSkinMaterial();
            Material darkGearMat = WCTMaterialPalette.GetPaddedVinylMaterial();
            Material shoeMat = WCTMaterialPalette.GetScaffoldRailMaterial();

            // 1. Torso / Jersey (y: 0.95m, size: 0.44 x 0.55 x 0.28)
            GameObject torso = CreateBodyPart("Torso_Jersey", visualsRoot.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.44f, 0.52f, 0.28f), null);
            jerseyRenderer = torso.GetComponent<MeshRenderer>();

            // 2. Head & Headband (y: 1.55m)
            GameObject head = CreateBodyPart("Head", visualsRoot.transform, new Vector3(0f, 1.52f, 0f), new Vector3(0.24f, 0.26f, 0.24f), skinMat);
            GameObject headband = CreateBodyPart("Headband", head.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.25f, 0.06f, 0.25f), shoeMat);

            // 3. Arms & Gloves
            // Left Arm
            CreateBodyPart("Left_Arm", visualsRoot.transform, new Vector3(-0.30f, 1.05f, 0f), new Vector3(0.12f, 0.45f, 0.12f), skinMat);
            CreateBodyPart("Left_Glove", visualsRoot.transform, new Vector3(-0.30f, 0.78f, 0f), new Vector3(0.13f, 0.12f, 0.13f), shoeMat);

            // Right Arm
            CreateBodyPart("Right_Arm", visualsRoot.transform, new Vector3(0.30f, 1.05f, 0f), new Vector3(0.12f, 0.45f, 0.12f), skinMat);
            CreateBodyPart("Right_Glove", visualsRoot.transform, new Vector3(0.30f, 0.78f, 0f), new Vector3(0.13f, 0.12f, 0.13f), shoeMat);

            // 4. Athletic Shorts (y: 0.68m)
            CreateBodyPart("Shorts", visualsRoot.transform, new Vector3(0f, 0.68f, 0f), new Vector3(0.42f, 0.24f, 0.26f), darkGearMat);

            // 5. Legs & Sneakers
            // Left Leg
            CreateBodyPart("Left_Leg", visualsRoot.transform, new Vector3(-0.13f, 0.38f, 0f), new Vector3(0.14f, 0.46f, 0.14f), skinMat);
            CreateBodyPart("Left_Sneaker", visualsRoot.transform, new Vector3(-0.13f, 0.08f, 0.04f), new Vector3(0.15f, 0.14f, 0.28f), shoeMat);

            // Right Leg
            CreateBodyPart("Right_Leg", visualsRoot.transform, new Vector3(0.13f, 0.38f, 0f), new Vector3(0.14f, 0.46f, 0.14f), skinMat);
            CreateBodyPart("Right_Sneaker", visualsRoot.transform, new Vector3(0.13f, 0.08f, 0.04f), new Vector3(0.15f, 0.14f, 0.28f), shoeMat);
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
            if (billboardTag != null)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    // Face billboard to camera
                    billboardTag.rotation = Quaternion.LookRotation(billboardTag.position - mainCamera.transform.position);

                    // Update live distance
                    float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
                    if (distTextMesh != null)
                    {
                        distTextMesh.text = $"{distance:0.0}m";
                    }
                }
            }
        }

        public void ApplyRoleColors(ChaseRole role)
        {
            currentRole = role;
            Material jerseyMat = (role == ChaseRole.Chaser)
                ? WCTMaterialPalette.GetChaserJerseyMaterial()
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

        private GameObject CreateBodyPart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.parent = parent;
            part.transform.localPosition = localPos;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;

            Collider col = part.GetComponent<Collider>();
            if (col != null) Destroy(col);

            MeshRenderer mr = part.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.material = mat;
            }

            return part;
        }
    }
}
