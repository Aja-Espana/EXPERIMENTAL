#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mandible.Registry
{
    public static partial class MandibleData
    {
        [MenuItem("Mandible/Data/Setup", false, priority = 0)]
        public static void HandleSetup()
        {
            string mandibleFolder = MandibleData.Setup();

            if (!string.IsNullOrEmpty(mandibleFolder))
            {
                EditorUtility.DisplayDialog(
                    "Mandible Setup Complete",
                    $"Mandible folder is ready at:\n{mandibleFolder}",
                    "OK"
                );
            }
        }
    }
}
#endif
