using UnityEngine;
using WorldChaseTag.Combat;
using WorldChaseTag.Visuals;

namespace WorldChaseTag.Arena
{
    /// <summary>
    /// Procedurally constructs the Championship World Chase Tag Quad Arena,
    /// featuring heavy-duty vinyl padded obstacles, steel scaffolding rails,
    /// team branded perimeter barriers (ALPHA cyan / OMEGA orange),
    /// an overhead suspended truss rig with stadium floodlights, and spectator tiers.
    /// Matched to the official WCT tournament arena reference.
    /// </summary>
    public class QuadArenaBuilder : MonoBehaviour
    {
        [Header("Arena Dimensions")]
        [SerializeField] private float arenaWidth = 12.0f;
        [SerializeField] private float arenaLength = 12.0f;
        [SerializeField] private float wallHeight = 2.4f;

        [Header("Spawn Points")]
        [SerializeField] private Transform chaserPlateSpawn;
        [SerializeField] private Transform evaderSpawn;

        public Transform ChaserPlateSpawn => chaserPlateSpawn;
        public Transform EvaderSpawn => evaderSpawn;

        [ContextMenu("Build Championship Quad Arena")]
        public void BuildArena()
        {
            // Clear existing children
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 1. Polished Court Floor (12m x 12m with white lane markings & yellow boundary)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ArenaFloor_RubberCourt";
            floor.transform.parent = transform;
            floor.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(arenaWidth, 0.5f, arenaLength);
            ApplyMaterial(floor, WCTMaterialPalette.GetFloorMaterial());

            // 2. Chaser Footprint Starting Plate
            GameObject plateObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObj.name = "ChaserStartingPlate";
            plateObj.transform.parent = transform;
            plateObj.transform.localPosition = new Vector3(0f, 0.02f, -5.2f);
            plateObj.transform.localScale = new Vector3(1.2f, 0.04f, 1.2f);
            DestroyImmediate(plateObj.GetComponent<Collider>());
            ApplyMaterial(plateObj, WCTMaterialPalette.GetChaserPlateMaterial(false));

            GameObject chaserSpawnObj = new GameObject("ChaserSpawnPoint");
            chaserSpawnObj.transform.parent = transform;
            chaserSpawnObj.transform.localPosition = new Vector3(0f, 0.1f, -5.2f);
            chaserSpawnObj.transform.localRotation = Quaternion.identity;
            chaserPlateSpawn = chaserSpawnObj.transform;

            // 3. Evader Starting Point (North quadrant)
            GameObject evaderSpawnObj = new GameObject("EvaderSpawnPoint");
            evaderSpawnObj.transform.parent = transform;
            evaderSpawnObj.transform.localPosition = new Vector3(0f, 0.1f, 3.2f);
            evaderSpawnObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            evaderSpawn = evaderSpawnObj.transform;

            // 4. Perimeter Branded Containment Barriers (ALPHA / OMEGA / WCT)
            CreateBrandedPerimeter();

            // 5. Central Mountain Obstacle (Stepped platform with vault ledges & ramp)
            CreateMountainObstacle(new Vector3(0f, 0f, 0f));

            // 6. Low Vault Hurdles (Black industrial steel rails with pipe fittings)
            CreateVaultHurdle(new Vector3(-3.2f, 0f, 1.8f), 0f);
            CreateVaultHurdle(new Vector3(3.2f, 0f, -1.8f), 0f);
            CreateVaultHurdle(new Vector3(-2.8f, 0f, -3.2f), 90f);
            CreateVaultHurdle(new Vector3(2.8f, 0f, 3.2f), 90f);

            // 7. High Mantle Blocks (Corner platforms at 1.6m height with top pads)
            CreatePaddedObstacleBlock("CornerMantle_NorthWest", new Vector3(-3.8f, 0f, 3.8f), new Vector3(2.2f, 1.6f, 1.8f));
            CreatePaddedObstacleBlock("CornerMantle_SouthEast", new Vector3(3.8f, 0f, -3.8f), new Vector3(2.2f, 1.6f, 1.8f));

            // 8. Side Obstacle Blocks (Choke points)
            CreatePaddedObstacleBlock("ChokeBlock_West", new Vector3(-3.8f, 0f, -0.5f), new Vector3(1.6f, 1.1f, 2.4f));
            CreatePaddedObstacleBlock("ChokeBlock_East", new Vector3(3.8f, 0f, 0.5f), new Vector3(1.6f, 1.1f, 2.4f));

            // 9. Overhead Suspended Steel Truss Rig & Stadium Spotlights
            CreateOverheadTrussAndLighting();

            // 10. Outer Stadium Seating Backdrop
            CreateStadiumAtmosphere();

            // 11. Boundary Enforcement Trigger
            GameObject boundaryObj = new GameObject("BoundaryTriggerVolume");
            boundaryObj.transform.parent = transform;
            boundaryObj.transform.localPosition = Vector3.zero;
            boundaryObj.AddComponent<WCTBoundaryTrigger>();
        }

        private void CreateBrandedPerimeter()
        {
            float halfW = arenaWidth * 0.5f;
            float halfL = arenaLength * 0.5f;

            // North Wall: WCT & ALPHA
            CreatePerimeterSection("Barrier_North", new Vector3(0f, wallHeight * 0.5f, halfL + 0.15f), new Vector3(arenaWidth + 0.3f, wallHeight, 0.3f), WCTMaterialPalette.GetAlphaBannerMaterial());
            // South Wall: WCT & OMEGA
            CreatePerimeterSection("Barrier_South", new Vector3(0f, wallHeight * 0.5f, -halfL - 0.15f), new Vector3(arenaWidth + 0.3f, wallHeight, 0.3f), WCTMaterialPalette.GetOmegaBannerMaterial());
            // West Wall: Team Alpha (Cyan)
            CreatePerimeterSection("Barrier_West", new Vector3(-halfW - 0.15f, wallHeight * 0.5f, 0f), new Vector3(0.3f, wallHeight, arenaLength), WCTMaterialPalette.GetAlphaBannerMaterial());
            // East Wall: Team Omega (Orange)
            CreatePerimeterSection("Barrier_East", new Vector3(halfW + 0.15f, wallHeight * 0.5f, 0f), new Vector3(0.3f, wallHeight, arenaLength), WCTMaterialPalette.GetOmegaBannerMaterial());
        }

        private void CreatePerimeterSection(string name, Vector3 pos, Vector3 size, Material bannerMat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = transform;
            wall.transform.localPosition = pos;
            wall.transform.localScale = size;
            ApplyMaterial(wall, bannerMat);

            // Steel Top Handrail
            GameObject handrail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handrail.name = name + "_TopRail";
            handrail.transform.parent = wall.transform;
            handrail.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            handrail.transform.localScale = new Vector3(1.02f, 0.08f, 1.25f);
            ApplyMaterial(handrail, WCTMaterialPalette.GetBlackSteelPipeMaterial());
        }

        private void CreateMountainObstacle(Vector3 center)
        {
            GameObject mountain = new GameObject("CentralMountain");
            mountain.transform.parent = transform;
            mountain.transform.localPosition = center;

            // Lower base step (0.75m high, 3.4m x 2.6m)
            CreateCompositeBlock(mountain.transform, "Mountain_BaseStep", new Vector3(0f, 0.375f, 0f), new Vector3(3.4f, 0.75f, 2.6f));

            // Upper tier (1.5m high, 2.0m x 1.6m)
            CreateCompositeBlock(mountain.transform, "Mountain_TopTier", new Vector3(0f, 1.125f, 0f), new Vector3(2.0f, 0.75f, 1.6f));

            // Angled balance ramp
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Mountain_Ramp";
            ramp.transform.parent = mountain.transform;
            ramp.transform.localPosition = new Vector3(1.8f, 0.35f, 0f);
            ramp.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            ramp.transform.localScale = new Vector3(1.4f, 0.15f, 1.2f);
            ApplyMaterial(ramp, WCTMaterialPalette.GetWhitePadMaterial());
        }

        private void CreatePaddedObstacleBlock(string name, Vector3 pos, Vector3 size)
        {
            GameObject blockRoot = new GameObject(name);
            blockRoot.transform.parent = transform;
            blockRoot.transform.localPosition = pos;

            CreateCompositeBlock(blockRoot.transform, name + "_Body", new Vector3(0f, size.y * 0.5f, 0f), size);
        }

        private void CreateCompositeBlock(Transform parent, string name, Vector3 localPos, Vector3 size)
        {
            // Vinyl padded core
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = name;
            body.transform.parent = parent;
            body.transform.localPosition = localPos;
            body.transform.localScale = size;
            ApplyMaterial(body, WCTMaterialPalette.GetPaddedVinylMaterial());

            // Non-slip top landing pad
            GameObject topPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topPad.name = name + "_TopPad";
            topPad.transform.parent = body.transform;
            topPad.transform.localPosition = new Vector3(0f, 0.505f, 0f);
            topPad.transform.localScale = new Vector3(0.96f, 0.04f, 0.96f);
            DestroyImmediate(topPad.GetComponent<Collider>());
            ApplyMaterial(topPad, WCTMaterialPalette.GetWhitePadMaterial());

            // Steel corner protective angle trims
            CreateCornerTrims(body.transform);
        }

        private void CreateCornerTrims(Transform body)
        {
            Material trimMat = WCTMaterialPalette.GetMetalTrimMaterial();
            float[] signsX = { -0.5f, 0.5f };
            float[] signsZ = { -0.5f, 0.5f };

            foreach (float sx in signsX)
            {
                foreach (float sz in signsZ)
                {
                    GameObject angle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    angle.name = "CornerBracket";
                    angle.transform.parent = body;
                    angle.transform.localPosition = new Vector3(sx * 0.99f, 0f, sz * 0.99f);
                    angle.transform.localScale = new Vector3(0.06f, 0.98f, 0.06f);
                    DestroyImmediate(angle.GetComponent<Collider>());
                    ApplyMaterial(angle, trimMat);
                }
            }
        }

        private void CreateVaultHurdle(Vector3 position, float yRotation)
        {
            GameObject hurdle = new GameObject("VaultHurdle");
            hurdle.transform.parent = transform;
            hurdle.transform.localPosition = position;
            hurdle.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            Material pipeMat = WCTMaterialPalette.GetBlackSteelPipeMaterial();
            Material chromeMat = WCTMaterialPalette.GetScaffoldRailMaterial();

            // Horizontal crossbar (0.85m high vault rail)
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "Hurdle_Bar";
            bar.transform.parent = hurdle.transform;
            bar.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            bar.transform.localScale = new Vector3(2.4f, 0.12f, 0.18f);
            ApplyMaterial(bar, pipeMat);

            // Chrome contact grip strip along top of rail
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Hurdle_GripStrip";
            grip.transform.parent = bar.transform;
            grip.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            grip.transform.localScale = new Vector3(0.92f, 0.08f, 0.65f);
            DestroyImmediate(grip.GetComponent<Collider>());
            ApplyMaterial(grip, chromeMat);

            // Left Post & Base Flange
            CreatePipeStanchion(hurdle.transform, "Hurdle_PostL", new Vector3(-1.1f, 0.425f, 0f), 0.85f, pipeMat);

            // Right Post & Base Flange
            CreatePipeStanchion(hurdle.transform, "Hurdle_PostR", new Vector3(1.1f, 0.425f, 0f), 0.85f, pipeMat);
        }

        private void CreatePipeStanchion(Transform parent, string name, Vector3 pos, float height, Material mat)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = name;
            post.transform.parent = parent;
            post.transform.localPosition = pos;
            post.transform.localScale = new Vector3(0.12f, height * 0.5f, 0.12f);
            ApplyMaterial(post, mat);

            // Floor base anchor flange
            GameObject flange = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flange.name = name + "_BaseFlange";
            flange.transform.parent = parent;
            flange.transform.localPosition = new Vector3(pos.x, 0.02f, pos.z);
            flange.transform.localScale = new Vector3(0.28f, 0.03f, 0.28f);
            DestroyImmediate(flange.GetComponent<Collider>());
            ApplyMaterial(flange, mat);

            // T-junction clamp fitting
            GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clamp.name = name + "_TClamp";
            clamp.transform.parent = parent;
            clamp.transform.localPosition = new Vector3(pos.x, pos.y + height * 0.5f, pos.z);
            clamp.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            DestroyImmediate(clamp.GetComponent<Collider>());
            ApplyMaterial(clamp, mat);
        }

        private void CreateOverheadTrussAndLighting()
        {
            GameObject rigRoot = new GameObject("Overhead_TrussRig");
            rigRoot.transform.parent = transform;
            rigRoot.transform.localPosition = new Vector3(0f, 6.5f, 0f);

            Material trussMat = WCTMaterialPalette.GetTrussMaterial();
            Material lensMat = WCTMaterialPalette.GetSpotlightLensMaterial();

            float halfDim = arenaWidth * 0.52f;

            // 4 Perimeter Main Steel Box Trusses
            CreateTrussBeam(rigRoot.transform, "Truss_North", new Vector3(0f, 0f, halfDim), new Vector3(arenaWidth + 2.5f, 0.45f, 0.45f), trussMat);
            CreateTrussBeam(rigRoot.transform, "Truss_South", new Vector3(0f, 0f, -halfDim), new Vector3(arenaWidth + 2.5f, 0.45f, 0.45f), trussMat);
            CreateTrussBeam(rigRoot.transform, "Truss_West", new Vector3(-halfDim, 0f, 0f), new Vector3(0.45f, 0.45f, arenaLength + 2.5f), trussMat);
            CreateTrussBeam(rigRoot.transform, "Truss_East", new Vector3(halfDim, 0f, 0f), new Vector3(0.45f, 0.45f, arenaLength + 2.5f), trussMat);

            // 8 Stadium Spotlights Angled Inward
            Vector3[] spotPositions = new Vector3[]
            {
                new Vector3(-4f, 0f, halfDim),
                new Vector3(4f, 0f, halfDim),
                new Vector3(-4f, 0f, -halfDim),
                new Vector3(4f, 0f, -halfDim),
                new Vector3(-halfDim, 0f, -3.5f),
                new Vector3(-halfDim, 0f, 3.5f),
                new Vector3(halfDim, 0f, -3.5f),
                new Vector3(halfDim, 0f, 3.5f)
            };

            for (int i = 0; i < spotPositions.Length; i++)
            {
                Vector3 p = spotPositions[i];
                GameObject lampObj = new GameObject($"SpotlightFixture_{i + 1}");
                lampObj.transform.parent = rigRoot.transform;
                lampObj.transform.localPosition = p;

                // Point towards court center
                Vector3 target = new Vector3(p.x * 0.25f, -6.5f, p.z * 0.25f);
                lampObj.transform.rotation = Quaternion.LookRotation(target - p);

                // Lamp Housing
                GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                housing.name = "LampHousing";
                housing.transform.parent = lampObj.transform;
                housing.transform.localPosition = Vector3.forward * 0.2f;
                housing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                housing.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
                DestroyImmediate(housing.GetComponent<Collider>());
                ApplyMaterial(housing, trussMat);

                // Glowing Lens
                GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lens.name = "LampLens";
                lens.transform.parent = housing.transform;
                lens.transform.localPosition = new Vector3(0f, 0.52f, 0f);
                lens.transform.localScale = new Vector3(0.92f, 0.05f, 0.92f);
                DestroyImmediate(lens.GetComponent<Collider>());
                ApplyMaterial(lens, lensMat);

                // Actual Unity Light
                Light spot = lampObj.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(1.0f, 0.96f, 0.90f);
                spot.intensity = 1.6f;
                spot.range = 22f;
                spot.spotAngle = 65f;
                spot.shadows = LightShadows.Soft;
            }

            // 4-sided Suspended Center Jumbotron Scoreboard Rig
            CreateCenterJumbotron(rigRoot.transform);
        }

        private void CreateCenterJumbotron(Transform parent)
        {
            GameObject jumbotronRoot = new GameObject("Overhead_Jumbotron");
            jumbotronRoot.transform.parent = parent;
            jumbotronRoot.transform.localPosition = new Vector3(0f, -1.8f, 0f);

            Material trussMat = WCTMaterialPalette.GetTrussMaterial();
            Material bannerMat = WCTMaterialPalette.GetWCTLogoBannerMaterial();

            // Central housing box
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Jumbotron_Housing";
            cube.transform.parent = jumbotronRoot.transform;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(3.4f, 1.8f, 3.4f);
            DestroyImmediate(cube.GetComponent<Collider>());
            ApplyMaterial(cube, trussMat);

            // 4 LED display screens (North, South, East, West)
            CreateJumbotronScreen(jumbotronRoot.transform, "Screen_North", new Vector3(0f, 0f, 1.72f), new Vector3(3.2f, 1.6f, 0.05f), bannerMat);
            CreateJumbotronScreen(jumbotronRoot.transform, "Screen_South", new Vector3(0f, 0f, -1.72f), new Vector3(3.2f, 1.6f, 0.05f), bannerMat);
            CreateJumbotronScreen(jumbotronRoot.transform, "Screen_West", new Vector3(-1.72f, 0f, 0f), new Vector3(0.05f, 1.6f, 3.2f), bannerMat);
            CreateJumbotronScreen(jumbotronRoot.transform, "Screen_East", new Vector3(1.72f, 0f, 0f), new Vector3(0.05f, 1.6f, 3.2f), bannerMat);

            // 4 downward spotlight corners
            Vector3[] downPositions = new Vector3[]
            {
                new Vector3(-1.5f, -0.9f, -1.5f),
                new Vector3(1.5f, -0.9f, -1.5f),
                new Vector3(-1.5f, -0.9f, 1.5f),
                new Vector3(1.5f, -0.9f, 1.5f)
            };

            foreach (var pos in downPositions)
            {
                GameObject downLightObj = new GameObject("Jumbotron_Downlight");
                downLightObj.transform.parent = jumbotronRoot.transform;
                downLightObj.transform.localPosition = pos;
                downLightObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                Light downLight = downLightObj.AddComponent<Light>();
                downLight.type = LightType.Spot;
                downLight.color = new Color(0.95f, 0.98f, 1.0f);
                downLight.intensity = 1.2f;
                downLight.range = 10f;
                downLight.spotAngle = 75f;
                downLight.shadows = LightShadows.Soft;
            }
        }

        private void CreateJumbotronScreen(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = name;
            screen.transform.parent = parent;
            screen.transform.localPosition = pos;
            screen.transform.localScale = size;
            DestroyImmediate(screen.GetComponent<Collider>());
            ApplyMaterial(screen, mat);
        }

        private void CreateTrussBeam(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = name;
            beam.transform.parent = parent;
            beam.transform.localPosition = pos;
            beam.transform.localScale = size;
            DestroyImmediate(beam.GetComponent<Collider>());
            ApplyMaterial(beam, mat);
        }

        private void CreateStadiumAtmosphere()
        {
            GameObject stadiumRoot = new GameObject("Stadium_SpectatorTiers");
            stadiumRoot.transform.parent = transform;
            stadiumRoot.transform.localPosition = Vector3.zero;

            Material standsMat = WCTMaterialPalette.GetStadiumStandsMaterial();
            Material wctBannerMat = WCTMaterialPalette.GetWCTLogoBannerMaterial();

            // Tiered dark spectator stands surrounding the arena
            float standDist = 11.5f;
            float standW = 28f;

            CreateStandTier(stadiumRoot.transform, "Stands_North", new Vector3(0f, 3.2f, standDist), new Vector3(standW, 4.5f, 5.0f), standsMat);
            CreateStandTier(stadiumRoot.transform, "Stands_South", new Vector3(0f, 3.2f, -standDist), new Vector3(standW, 4.5f, 5.0f), standsMat);
            CreateStandTier(stadiumRoot.transform, "Stands_West", new Vector3(-standDist, 3.2f, 0f), new Vector3(5.0f, 4.5f, standW), standsMat);
            CreateStandTier(stadiumRoot.transform, "Stands_East", new Vector3(standDist, 3.2f, 0f), new Vector3(5.0f, 4.5f, standW), standsMat);

            // Large vertical WCT Championship banners hanging in background matching Image 1
            CreateHangingBanner(stadiumRoot.transform, "WCT_Banner_NorthWest", new Vector3(-5.5f, 5.0f, 6.8f), wctBannerMat);
            CreateHangingBanner(stadiumRoot.transform, "WCT_Banner_NorthEast", new Vector3(5.5f, 5.0f, 6.8f), wctBannerMat);
            CreateHangingBanner(stadiumRoot.transform, "WCT_Banner_SouthWest", new Vector3(-5.5f, 5.0f, -6.8f), wctBannerMat);
            CreateHangingBanner(stadiumRoot.transform, "WCT_Banner_SouthEast", new Vector3(5.5f, 5.0f, -6.8f), wctBannerMat);
        }

        private void CreateStandTier(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject tier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tier.name = name;
            tier.transform.parent = parent;
            tier.transform.localPosition = pos;
            tier.transform.localScale = size;
            DestroyImmediate(tier.GetComponent<Collider>());
            ApplyMaterial(tier, mat);
        }

        private void CreateHangingBanner(Transform parent, string name, Vector3 pos, Material mat)
        {
            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            banner.name = name;
            banner.transform.parent = parent;
            banner.transform.localPosition = pos;
            banner.transform.localScale = new Vector3(1.8f, 3.6f, 0.05f);
            DestroyImmediate(banner.GetComponent<Collider>());
            ApplyMaterial(banner, mat);
        }

        private void ApplyMaterial(GameObject obj, Material mat)
        {
            if (obj == null || mat == null) return;
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = mat;
            }
        }
    }
}
