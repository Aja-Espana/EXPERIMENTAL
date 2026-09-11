using UnityEngine;
using UnityEditor;

public class CubeSphereMenu
{
    [MenuItem("GameObject/3D Object/Cube Sphere", false, 10)]
    private static void CreateCubeSphere(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("CubeSphere");

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        CubeSphereMesh cubeSphere = go.AddComponent<CubeSphereMesh>();
        cubeSphere.GenerateCubeSphere();

        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) 
        { 
            color = Color.gray 
        };

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create Cube Sphere");
        Selection.activeGameObject = go;
    }
}