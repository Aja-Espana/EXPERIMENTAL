#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Mandible.Registry
{
    public static partial class MandibleData
    {
        private const string RootLabel = "Mandible_DataRoot";
        private const string RootAssetName = "MandibleDataRoot.asset";

        //Folders

        public static string FindFolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:Folder Mandible");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            return null;
        }

        //Root

        public static bool TryGetRoot(out MandibleDataRoot root, out string path)
        {
            var guids = AssetDatabase.FindAssets(
                $"l:{RootLabel} t:{nameof(MandibleDataRoot)}"
            );

            if (guids.Length == 0)
            {
                root = null;
                path = null;
                return false;
            }

            if (guids.Length > 1)
            {
                Debug.LogError("Multiple Mandible Data Roots found.");
            }

            path = AssetDatabase.GUIDToAssetPath(guids[0]);
            root = AssetDatabase.LoadAssetAtPath<MandibleDataRoot>(path);

            return root != null;
        }

        public static bool TryGetRoot(out MandibleDataRoot root)
        {
            return TryGetRoot(out root, out _);
        }

        public static bool TryGetRoot(out string path)
        {
            return TryGetRoot(out _, out path);
        }

        public static bool TryGetRoot()
        {
            return TryGetRoot(out _, out _);
        }

        public static string GetRootFolder()
        {
            if (!TryGetRoot(out var root, out var path))
                return null;

            //var path = AssetDatabase.GetAssetPath(root);
            return System.IO.Path.GetDirectoryName(path);
        }

        //Assets

        public static T EnsureAsset<T>(string fullPath) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogError("MandibleData.EnsureAsset: fullPath is null or empty.");
                return null;
            }

            fullPath = fullPath.Replace("\\", "/");

            var asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();

            string folder = Path.GetDirectoryName(fullPath).Replace("\\", "/");
            CreateFolder(folder);

            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created asset {typeof(T).Name} at {fullPath}");
            return asset;
        }

        public static T EnsureDataAsset<T>(string relativePath) where T : ScriptableObject
        {
            string rootFolder = GetRootFolder();
            if (string.IsNullOrEmpty(rootFolder)) return null;
            
            string fullPath = Path.Combine(rootFolder, "Data", relativePath).Replace("\\", "/");
            if (!fullPath.EndsWith(".asset")) fullPath += ".asset";

            return EnsureAsset<T>(fullPath);
        }
    }
}
#endif
