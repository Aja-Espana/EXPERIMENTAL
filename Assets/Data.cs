using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public static class Data
{
    // Seeds
    public static class Seeds
    {
        public static int GenerateSeed()
        {
            return 0;
        }
    }

    // Star Maps
    public static class StarMap
    {
        public static Texture2D GenerateTexture(int width, int height, float density, int seed = 0)
        {
            if (seed != 0) 
            {
                Random.InitState(seed);
            }

            Texture2D map = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    if (Random.value < density)
                    {
                        float localX = Random.value;
                        float localY = Random.value;
                        int colorIdx = Random.Range(0, 8);
                        int size = Random.Range(1, 9);

                        pixels[index] = new Color(localX, localY, colorIdx, size);
                    }
                    else
                    {
                        pixels[index] = Color.clear;
                    }
                }
            }

            map.SetPixels(pixels);
            map.Apply();

            return map;
        }

        public static StarMapData GenerateData(int width, int height, float density, int seed = 0)
        {
            if (seed != 0) 
            {
                Random.InitState(seed);
            }

            StarMapData data = new StarMapData(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    if (Random.value < density)
                    {
                        float localX = Random.value;
                        float localY = Random.value;
                        int colorIdx = Random.Range(0, 8);
                        int size = Random.Range(1, 9);

                        data.data[index] = new Vector4(localX, localY, colorIdx, size);
                    }
                    else
                    {
                        data.data[index] = Vector4.zero;
                    }
                }
            }

            return data;
        }

    #if UNITY_EDITOR
        [MenuItem("Tools/Star Map/Generate & Save New Star Map")]
        private static void EditorGenerateAndSave()
        {
            int randomSeed = Random.Range(1000, 9999);
            Texture2D generatedTex = GenerateTexture(256, 256, 0.0025f, randomSeed);

            string path = $"Assets/GeneratedStarMap_{randomSeed}.png";

            byte[] bytes = generatedTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }

            Object.DestroyImmediate(generatedTex);

            Debug.Log($"<color=cyan>Star Map successfully saved to:</color> {path} (Seed: {randomSeed})");
            
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
    #endif
    }
}
