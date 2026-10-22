#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WorldChaseTag.Editor
{
    public static class SnapsAssetInspector
    {
        [MenuItem("World Chase Tag/Inspect Snaps Dimensions")]
        public static void InspectDimensions()
        {
            string[] prefabPaths = new string[]
            {
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Props/GuardRail_01_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Props/GuardRail_02_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Structure/Stairs_Ramps/Quart_Ramp_01_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Structure/Stairs_Ramps/Gen_Ramp_01_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Structure/Floors_Ceilings/Deck_Floor_01_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Structure/Pillars_Supports/VerticalStrat_01_snaps002.prefab",
                "Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/SciFi_Industrial/Prefabs/Decor/Crossbar_01_snaps002.prefab"
            };

            foreach (var path in prefabPaths)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj == null)
                {
                    Debug.LogWarning($"Could not load: {path}");
                    continue;
                }

                MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Bounds b = mf.sharedMesh.bounds;
                    Debug.Log($"[Snaps Inspect] {obj.name}: Mesh Bounds size={b.size}, center={b.center}");
                }
            }
        }
    }
}
#endif
