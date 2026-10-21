using UnityEngine;

namespace WorldChaseTag.Visuals
{
    /// <summary>
    /// Generates high-spec PBR materials, procedural textures, and visual palettes
    /// for the World Chase Tag Quad arena, obstacles, athletes, and stadium environment.
    /// Supports both Universal Render Pipeline (URP) and Standard Built-In PBR.
    /// </summary>
    public static class WCTMaterialPalette
    {
        private static Material floorMat;
        private static Material paddedVinylMat;
        private static Material metalTrimMat;
        private static Material whitePadMat;
        private static Material blackSteelPipeMat;
        private static Material scaffoldRailMat;
        private static Material alphaBannerMat;
        private static Material omegaBannerMat;
        private static Material wctBannerMat;
        private static Material trussMat;
        private static Material spotlightLensMat;
        private static Material chaserPlateMat;
        private static Material chaserJerseyMat;
        private static Material evaderJerseyMat;
        private static Material skinMat;
        private static Material realisticSkinMat;
        private static Material sportsWatchMat;
        private static Material watchScreenMat;
        private static Material fingernailMat;
        private static Material obstacleBlueTrimMat;
        private static Material omegaJerseyMat;
        private static Material stadiumStandsMat;

        public static Material GetFloorMaterial()
        {
            if (floorMat == null)
            {
                // Dark polished athletic court floor with bright white markings matching Image 1
                floorMat = CreatePBRMaterial("WCT_Floor_ReflectiveCourt", new Color(0.12f, 0.14f, 0.17f), 0.20f, 0.68f);
                SetTexture(floorMat, GenerateCourtTexture(1024, 1024));
                SetNormalMap(floorMat, GenerateNormalMapFromHeight(256, 256, FloorHeightSampler, 2.2f), 1.0f);
            }
            return floorMat;
        }

        public static Material GetPaddedVinylMaterial()
        {
            if (paddedVinylMat == null)
            {
                // Heavy-duty black vinyl obstacle padding with safety seams matching Image 1
                paddedVinylMat = CreatePBRMaterial("WCT_Obstacle_PaddedVinyl", new Color(0.11f, 0.12f, 0.14f), 0.04f, 0.42f);
                SetTexture(paddedVinylMat, GenerateVinylPaddingTexture(512, 512));
                SetNormalMap(paddedVinylMat, GenerateNormalMapFromHeight(256, 256, VinylHeightSampler, 2.8f), 1.2f);
            }
            return paddedVinylMat;
        }

        public static Material GetObstacleBaseMaterial() => GetPaddedVinylMaterial();

        public static Material GetMetalTrimMaterial()
        {
            if (metalTrimMat == null)
            {
                // Steel edge angle guards and corner brackets
                metalTrimMat = CreatePBRMaterial("WCT_Metal_CornerTrim", new Color(0.24f, 0.26f, 0.29f), 0.88f, 0.78f);
                SetNormalMap(metalTrimMat, GenerateNormalMapFromHeight(128, 128, BrushedMetalHeightSampler, 1.8f), 0.9f);
            }
            return metalTrimMat;
        }

        public static Material GetWhitePadMaterial()
        {
            if (whitePadMat == null)
            {
                // High-traction light-grey / white top grip surface matching Image 1
                whitePadMat = CreatePBRMaterial("WCT_Obstacle_WhitePad", new Color(0.85f, 0.87f, 0.89f), 0.02f, 0.35f);
                SetTexture(whitePadMat, GenerateGripPadTexture(512, 512));
                SetNormalMap(whitePadMat, GenerateNormalMapFromHeight(256, 256, DiamondHeightSampler, 3.2f), 1.4f);
            }
            return whitePadMat;
        }

        public static Material GetBlackSteelPipeMaterial()
        {
            if (blackSteelPipeMat == null)
            {
                // Dark industrial steel scaffolding rails and pipe clamps matching Image 1
                blackSteelPipeMat = CreatePBRMaterial("WCT_Steel_ScaffoldingPipe", new Color(0.15f, 0.16f, 0.18f), 0.88f, 0.82f);
                SetNormalMap(blackSteelPipeMat, GenerateNormalMapFromHeight(128, 128, BrushedMetalHeightSampler, 1.8f), 0.9f);
            }
            return blackSteelPipeMat;
        }

        public static Material GetScaffoldRailMaterial()
        {
            if (scaffoldRailMat == null)
            {
                // Brushed chrome / metallic vault rail accent
                scaffoldRailMat = CreatePBRMaterial("WCT_Scaffold_ChromeRail", new Color(0.80f, 0.83f, 0.86f), 0.95f, 0.90f);
                SetNormalMap(scaffoldRailMat, GenerateNormalMapFromHeight(128, 128, BrushedMetalHeightSampler, 1.5f), 0.75f);
            }
            return scaffoldRailMat;
        }

        public static Material GetAlphaBannerMaterial()
        {
            if (alphaBannerMat == null)
            {
                // Team Alpha: Electric Cyan-Blue with diagonal sport graphics
                alphaBannerMat = CreatePBRMaterial("WCT_Banner_ALPHA", new Color(0.06f, 0.55f, 0.85f), 0.1f, 0.45f);
                SetTexture(alphaBannerMat, GenerateTeamBannerTexture(512, 256, "ALPHA", new Color(0.05f, 0.50f, 0.82f), Color.white));
            }
            return alphaBannerMat;
        }

        public static Material GetOmegaBannerMaterial()
        {
            if (omegaBannerMat == null)
            {
                // Team Omega: Electric Orange with chevron sport graphics
                omegaBannerMat = CreatePBRMaterial("WCT_Banner_OMEGA", new Color(0.92f, 0.38f, 0.08f), 0.1f, 0.45f);
                SetTexture(omegaBannerMat, GenerateTeamBannerTexture(512, 256, "OMEGA", new Color(0.88f, 0.36f, 0.08f), Color.white));
            }
            return omegaBannerMat;
        }

        public static Material GetWCTLogoBannerMaterial()
        {
            if (wctBannerMat == null)
            {
                // Official WCT Tournament Black & White Barrier Print
                wctBannerMat = CreatePBRMaterial("WCT_Banner_Official", new Color(0.08f, 0.09f, 0.11f), 0.15f, 0.4f);
                SetTexture(wctBannerMat, GenerateWCTLogoTexture(512, 256));
            }
            return wctBannerMat;
        }

        public static Material GetTrussMaterial()
        {
            if (trussMat == null)
            {
                // Matte dark steel structural ceiling truss
                trussMat = CreatePBRMaterial("WCT_Arena_Truss", new Color(0.12f, 0.13f, 0.15f), 0.7f, 0.5f);
            }
            return trussMat;
        }

        public static Material GetSpotlightLensMaterial()
        {
            if (spotlightLensMat == null)
            {
                spotlightLensMat = CreatePBRMaterial("WCT_Spotlight_Lens", Color.white, 0.1f, 0.95f);
                SetEmission(spotlightLensMat, new Color(1.0f, 0.96f, 0.88f) * 3.5f);
            }
            return spotlightLensMat;
        }

        public static Material GetChaserPlateMaterial(bool isActive = false)
        {
            if (chaserPlateMat == null)
            {
                chaserPlateMat = CreatePBRMaterial("WCT_Chaser_Plate", new Color(0.10f, 0.12f, 0.15f), 0.6f, 0.8f);
            }
            Color emission = isActive ? new Color(0.1f, 0.95f, 0.35f) * 2.0f : new Color(0.95f, 0.4f, 0.1f) * 1.5f;
            SetEmission(chaserPlateMat, emission);
            return chaserPlateMat;
        }

        public static Material GetChaserJerseyMaterial()
        {
            if (chaserJerseyMat == null)
            {
                chaserJerseyMat = CreatePBRMaterial("WCT_Jersey_Chaser", new Color(0.95f, 0.36f, 0.08f), 0.1f, 0.5f);
                SetEmission(chaserJerseyMat, new Color(0.95f, 0.36f, 0.08f) * 0.45f);
            }
            return chaserJerseyMat;
        }

        public static Material GetEvaderJerseyMaterial()
        {
            if (evaderJerseyMat == null)
            {
                evaderJerseyMat = CreatePBRMaterial("WCT_Jersey_Evader", new Color(0.08f, 0.68f, 0.95f), 0.1f, 0.5f);
                SetEmission(evaderJerseyMat, new Color(0.08f, 0.68f, 0.95f) * 0.45f);
            }
            return evaderJerseyMat;
        }

        public static Material GetGloveMaterial(Color accentColor)
        {
            Material m = CreatePBRMaterial("WCT_Athlete_Glove", new Color(0.12f, 0.13f, 0.15f), 0.3f, 0.6f);
            SetEmission(m, accentColor * 0.5f);
            return m;
        }

        public static Material GetSkinMaterial()
        {
            if (skinMat == null)
            {
                skinMat = CreatePBRMaterial("WCT_Athlete_Skin", new Color(0.78f, 0.58f, 0.46f), 0.0f, 0.25f);
            }
            return skinMat;
        }

        public static Material GetRealisticSkinMaterial()
        {
            if (realisticSkinMat == null)
            {
                // Photorealistic athletic skin with micro-creases and warm epidermal tones
                realisticSkinMat = CreatePBRMaterial("WCT_Photorealistic_Skin", new Color(0.84f, 0.62f, 0.49f), 0.0f, 0.40f);
                SetTexture(realisticSkinMat, GenerateSkinAlbedoTexture(512, 512));
                SetNormalMap(realisticSkinMat, GenerateNormalMapFromHeight(256, 256, SkinHeightSampler, 2.2f), 1.15f);
            }
            return realisticSkinMat;
        }

        public static Material GetSportsWatchMaterial()
        {
            if (sportsWatchMat == null)
            {
                // Matte black sports silicone casing & strap matching screenshot
                sportsWatchMat = CreatePBRMaterial("WCT_SportsWatch_Silicone", new Color(0.12f, 0.13f, 0.15f), 0.35f, 0.52f);
                SetNormalMap(sportsWatchMat, GenerateNormalMapFromHeight(128, 128, BrushedMetalHeightSampler, 1.6f), 0.8f);
            }
            return sportsWatchMat;
        }

        public static Material GetWatchScreenMaterial()
        {
            if (watchScreenMat == null)
            {
                // High-tech digital LCD chronograph display with cyan athletic telemetry
                watchScreenMat = CreatePBRMaterial("WCT_SportsWatch_Screen", new Color(0.04f, 0.07f, 0.10f), 0.1f, 0.95f);
                SetTexture(watchScreenMat, GenerateWatchScreenTexture(256, 256));
                SetEmission(watchScreenMat, new Color(0.05f, 0.85f, 0.95f) * 0.95f);
            }
            return watchScreenMat;
        }

        public static Material GetFingernailMaterial()
        {
            if (fingernailMat == null)
            {
                // Glossy natural keratin fingernail plate
                fingernailMat = CreatePBRMaterial("WCT_Fingernail_Keratin", new Color(0.88f, 0.74f, 0.70f), 0.05f, 0.82f);
            }
            return fingernailMat;
        }

        public static Material GetObstacleBlueTrimMaterial()
        {
            if (obstacleBlueTrimMat == null)
            {
                // Striking electric cyan/blue obstacle edge trim matching screenshot
                obstacleBlueTrimMat = CreatePBRMaterial("WCT_Obstacle_BlueTrim", new Color(0.05f, 0.62f, 0.98f), 0.15f, 0.72f);
                SetEmission(obstacleBlueTrimMat, new Color(0.05f, 0.62f, 0.98f) * 0.35f);
            }
            return obstacleBlueTrimMat;
        }

        public static Material GetOmegaJerseyMaterial()
        {
            if (omegaJerseyMat == null)
            {
                // Deep athletic red/burgundy jersey featuring the white Omega Ω logo matching screenshot
                omegaJerseyMat = CreatePBRMaterial("WCT_Jersey_Omega_Reference", new Color(0.68f, 0.15f, 0.15f), 0.08f, 0.42f);
                SetTexture(omegaJerseyMat, GenerateOmegaJerseyTexture(512, 512));
                SetNormalMap(omegaJerseyMat, GenerateNormalMapFromHeight(256, 256, VinylHeightSampler, 1.4f), 0.85f);
            }
            return omegaJerseyMat;
        }

        public static Material GetStadiumStandsMaterial()
        {
            if (stadiumStandsMat == null)
            {
                stadiumStandsMat = CreatePBRMaterial("WCT_Stadium_Stands", new Color(0.05f, 0.06f, 0.08f), 0.2f, 0.4f);
            }
            return stadiumStandsMat;
        }

        private static Material CreatePBRMaterial(string name, Color albedo, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            Material mat = new Material(shader)
            {
                name = name
            };

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", albedo);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            return mat;
        }

        private static void SetTexture(Material mat, Texture2D tex)
        {
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        }

        private static void SetEmission(Material mat, Color emissionColor)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
            }
        }

        private static void SetNormalMap(Material mat, Texture2D normalTex, float bumpScale = 1.0f)
        {
            if (mat == null || normalTex == null) return;
            if (mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalTex);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", bumpScale);
            }
        }

        private static Texture2D GenerateNormalMapFromHeight(int width, int height, System.Func<int, int, float> heightSampler, float strength = 2.0f)
        {
            Texture2D normTex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float hL = heightSampler((x - 1 + width) % width, y);
                    float hR = heightSampler((x + 1) % width, y);
                    float hD = heightSampler(x, (y - 1 + height) % height);
                    float hU = heightSampler(x, (y + 1) % height);

                    float dX = (hR - hL) * strength;
                    float dY = (hU - hD) * strength;
                    Vector3 normal = new Vector3(-dX, -dY, 1.0f).normalized;

                    normTex.SetPixel(x, y, new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f, 1.0f));
                }
            }
            normTex.Apply();
            return normTex;
        }

        private static float FloorHeightSampler(int x, int y)
        {
            float grain = ((x * 37 + y * 19) % 13) / 13.0f * 0.08f;
            bool isTile = (x % 32 == 0 || y % 32 == 0);
            return isTile ? -0.2f : grain;
        }

        private static float VinylHeightSampler(int x, int y)
        {
            float dx = Mathf.Min(x, 255 - x) / 24f;
            float dy = Mathf.Min(y, 255 - y) / 24f;
            float edgeDist = Mathf.Min(dx, dy);
            float pillow = Mathf.Clamp01(edgeDist);
            bool isSeam = (x >= 16 && x <= 20) || (x >= 235 && x <= 239) || (y >= 16 && y <= 20) || (y >= 235 && y <= 239);
            float leatherGrain = ((x * 29 + y * 31) % 11) / 11.0f * 0.05f;
            return (isSeam ? -0.3f : pillow * 0.4f) + leatherGrain;
        }

        private static float DiamondHeightSampler(int x, int y)
        {
            int d1 = (x + y) % 12;
            int d2 = (x - y + 1000) % 12;
            return (d1 <= 2 || d2 <= 2) ? 0.35f : 0.0f;
        }

        private static float BrushedMetalHeightSampler(int x, int y)
        {
            return Mathf.Sin(x * 0.8f) * 0.06f + ((y * 17) % 7) * 0.02f;
        }

        // --- Procedural Texture Generators Matching Image 1 ---

        private static Texture2D GenerateCourtTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkCourt = new Color(0.12f, 0.14f, 0.17f);
            Color courtSheen = new Color(0.15f, 0.17f, 0.20f);
            Color whiteLine = new Color(0.92f, 0.94f, 0.96f);
            Color yellowBorder = new Color(0.95f, 0.78f, 0.12f);

            int borderWidth = width / 18;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = ((x + y) % 32 < 16) ? darkCourt : courtSheen;

                    // Grid lines (athletic court tiles)
                    if (x % (width / 12) <= 1 || y % (height / 12) <= 1)
                    {
                        pixel = new Color(0.18f, 0.21f, 0.25f);
                    }

                    // Outer perimeter yellow line (12m WCT legal playing area)
                    if ((x >= borderWidth - 6 && x <= borderWidth) ||
                        (x >= width - borderWidth && x <= width - borderWidth + 6) ||
                        (y >= borderWidth - 6 && y <= borderWidth) ||
                        (y >= height - borderWidth && y <= height - borderWidth + 6))
                    {
                        pixel = yellowBorder;
                    }

                    // Internal white runway lines (matching Image 1!)
                    // Longitudinal lines
                    if ((x >= width * 0.25f - 4 && x <= width * 0.25f + 4) ||
                        (x >= width * 0.75f - 4 && x <= width * 0.75f + 4))
                    {
                        if (y >= borderWidth && y <= height - borderWidth)
                        {
                            pixel = whiteLine;
                        }
                    }

                    // Transverse lines
                    if ((y >= height * 0.35f - 4 && y <= height * 0.35f + 4) ||
                        (y >= height * 0.65f - 4 && y <= height * 0.65f + 4))
                    {
                        if (x >= borderWidth && x <= width - borderWidth)
                        {
                            pixel = whiteLine;
                        }
                    }

                    // Center Octagon / Emblem
                    float dx = x - width * 0.5f;
                    float dy = y - height * 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > 75f && dist < 85f)
                    {
                        pixel = whiteLine;
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateVinylPaddingTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color vinylBase = new Color(0.11f, 0.12f, 0.14f);
            Color seamDark = new Color(0.06f, 0.07f, 0.08f);
            Color stitch = new Color(0.24f, 0.26f, 0.29f);

            int seamOffset = 18;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = vinylBase;

                    // Subtle fabric / leather micro-texture
                    float noise = ((x * 19 + y * 23) % 11) * 0.008f;
                    pixel = new Color(pixel.r + noise, pixel.g + noise, pixel.b + noise, 1f);

                    // Padded edge seams
                    if (x < seamOffset || x >= width - seamOffset || y < seamOffset || y >= height - seamOffset)
                    {
                        if (x == seamOffset - 2 || x == width - seamOffset + 2 || y == seamOffset - 2 || y == height - seamOffset + 2)
                        {
                            pixel = seamDark;
                        }
                        else if ((x == seamOffset || x == width - seamOffset || y == seamOffset || y == height - seamOffset) && ((x + y) % 8 < 4))
                        {
                            pixel = stitch;
                        }
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateGripPadTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color padBase = new Color(0.82f, 0.84f, 0.86f);
            Color border = new Color(0.18f, 0.20f, 0.22f);

            int bWidth = 14;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < bWidth || x >= width - bWidth || y < bWidth || y >= height - bWidth);
                    Color pixel = isBorder ? border : padBase;

                    // Non-slip diamond grain
                    if (!isBorder && ((x + y) % 6 == 0 || (x - y + 1000) % 6 == 0))
                    {
                        pixel = new Color(padBase.r - 0.04f, padBase.g - 0.04f, padBase.b - 0.04f, 1f);
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateTeamBannerTexture(int width, int height, string teamName, Color teamColor, Color textColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color bgDark = new Color(0.08f, 0.09f, 0.11f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = bgDark;

                    // Diagonal racing stripes matching Image 1
                    int diag = (x + y * 2) % 120;
                    if (diag < 45)
                    {
                        pixel = teamColor;
                    }
                    else if (diag < 52)
                    {
                        pixel = Color.black;
                    }

                    // Bold horizontal sponsor band
                    if (y >= height * 0.38f && y <= height * 0.62f)
                    {
                        if (x >= width * 0.35f && x <= width * 0.85f)
                        {
                            pixel = bgDark * 0.6f;
                        }
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWCTLogoTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color bgDark = new Color(0.08f, 0.09f, 0.11f);
            Color logoWhite = new Color(0.85f, 0.88f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = bgDark;

                    // Centered circular globe logo
                    float dx = x - width * 0.5f;
                    float dy = (y - height * 0.5f) * 1.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > 45f && dist < 52f)
                    {
                        pixel = logoWhite;
                    }
                    else if (dist < 45f)
                    {
                        // Latitudinal lines
                        if (Mathf.Abs(dy) < 3f || Mathf.Abs(dy - 20f) < 2f || Mathf.Abs(dy + 20f) < 2f)
                        {
                            pixel = logoWhite;
                        }
                        if (Mathf.Abs(dx) < 3f || Mathf.Abs(dx * 1.4f - dy) < 2f || Mathf.Abs(dx * 1.4f + dy) < 2f)
                        {
                            pixel = logoWhite;
                        }
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        private static float SkinHeightSampler(int x, int y)
        {
            // Knuckle and flexor crease lines
            float crease = 0f;
            if (y % 64 < 4) crease -= 0.25f;
            if (y % 128 < 6) crease -= 0.35f;

            // Micro epidermal pores & hair follicle stippling
            float pores = ((x * 47 + y * 83) % 19) / 19.0f * 0.06f;
            float cellular = Mathf.Sin(x * 0.4f) * Mathf.Cos(y * 0.4f) * 0.04f;
            return crease + pores + cellular;
        }

        private static Texture2D GenerateSkinAlbedoTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color skinBase = new Color(0.85f, 0.64f, 0.52f);
            Color skinWarm = new Color(0.82f, 0.52f, 0.42f); // Warm pinkish tone on knuckles
            Color palmLight = new Color(0.89f, 0.72f, 0.62f);

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float normX = (float)x / width;
                    Color pixel = Color.Lerp(skinBase, palmLight, normX * 0.3f);

                    // Knuckle warm tone bands
                    float knuckleDist = Mathf.Abs(Mathf.Sin(normY * Mathf.PI * 4f));
                    if (knuckleDist > 0.7f)
                    {
                        pixel = Color.Lerp(pixel, skinWarm, (knuckleDist - 0.7f) * 1.5f);
                    }

                    // Micro noise
                    float noise = ((x * 31 + y * 41) % 13) * 0.004f;
                    pixel = new Color(pixel.r + noise, pixel.g + noise * 0.5f, pixel.b + noise * 0.3f, 1f);

                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWatchScreenTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color screenBg = new Color(0.04f, 0.07f, 0.10f);
            Color cyanGlow = new Color(0.08f, 0.95f, 0.88f);
            Color dimCyan = new Color(0.04f, 0.35f, 0.32f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = screenBg;

                    // Outer bezel border
                    if (x < 12 || x >= width - 12 || y < 12 || y >= height - 12)
                    {
                        pixel = new Color(0.02f, 0.03f, 0.04f);
                    }
                    // Telemetry Header bar
                    else if (y >= height - 38 && y <= height - 34)
                    {
                        pixel = dimCyan;
                    }
                    // Center Big Digits area: "00:14"
                    else if (y >= height * 0.38f && y <= height * 0.62f)
                    {
                        int segX = x % 48;
                        if ((segX >= 10 && segX <= 14) || (segX >= 34 && segX <= 38) || (y >= height * 0.48f && y <= height * 0.52f))
                        {
                            pixel = cyanGlow;
                        }
                    }
                    // Bottom Heart Rate telemetry
                    else if (y >= 26 && y <= 34)
                    {
                        if (x >= 28 && x <= 80) pixel = cyanGlow;
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateOmegaJerseyTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color jerseyRed = new Color(0.64f, 0.12f, 0.12f);
            Color fabricWeave = new Color(0.58f, 0.10f, 0.10f);
            Color omegaWhite = new Color(0.96f, 0.96f, 0.96f);

            float cx = width * 0.5f;
            float cy = height * 0.55f;
            float outerR = width * 0.20f;
            float innerR = width * 0.13f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = ((x + y) % 4 < 2) ? jerseyRed : fabricWeave;

                    // Large Omega "Ω" Symbol on athlete's back
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Main circular horseshoe arch
                    if (dist >= innerR && dist <= outerR)
                    {
                        // Exclude bottom gap (between -70 and -110 deg)
                        float angleDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (!(angleDeg < -60f && angleDeg > -120f))
                        {
                            pixel = omegaWhite;
                        }
                    }

                    // Left & Right Horizontal Feet of Omega Ω
                    float feetY = cy - outerR * 0.88f;
                    if (Mathf.Abs(y - feetY) <= (outerR - innerR) * 0.5f)
                    {
                        // Left foot
                        if (x >= cx - outerR * 1.35f && x <= cx - innerR * 0.75f)
                        {
                            pixel = omegaWhite;
                        }
                        // Right foot
                        if (x >= cx + innerR * 0.75f && x <= cx + outerR * 1.35f)
                        {
                            pixel = omegaWhite;
                        }
                    }

                    // "WCT" Text Band above Omega symbol
                    if (y >= height * 0.82f && y <= height * 0.86f && Mathf.Abs(x - cx) < width * 0.22f)
                    {
                        pixel = omegaWhite;
                    }

                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
