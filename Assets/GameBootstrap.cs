using UnityEngine;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeGame()
    {
        Debug.Log("Booting up game systems...");
        GameContext.Generate();
        Debug.Log("Core game data initialized successfully.");
    }
}