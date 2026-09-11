using UnityEngine;

public static class GameContext
{
    public static GameContextData ctx;
    private static bool hasGeneratedContext = false;

    public static void Generate()
    {
        // Ensure only created once
        if (ctx != null) return;
        if (hasGeneratedContext) return;

        // Context
        ctx = new GameContextData();

        hasGeneratedContext = true;

        Debug.Log("Generated Context");
    }
}

public class GameContextData
{
    public Texture2D starMapTexture;

    public GameContextData() {
        starMapTexture = Data.StarMap.GenerateTexture(256, 256, 0.001f);
    }
}
