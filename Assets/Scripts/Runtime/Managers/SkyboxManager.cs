using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SkyboxManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Transform sky;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Gradient _ambientGradient;

    [Header("sky Angles")]
    [SerializeField] private float sunYRotation = 170f;

    Material skybox;

    void Start()
    {
        skybox = RenderSettings.skybox;
    }

    void OnEnable()
    {
        skybox = RenderSettings.skybox;
    }

    void Update()
    {
        if (skybox == null || sky == null) return;

        UpdateSun();
    }

    public void UpdateSun()
    {
        if (skybox == null || sky == null) return;

        float angle = sky.transform.eulerAngles.x;
        float t;

        if (angle > 180f)
        {
            t = 1.0f;
        }
        else
        {
            float distFromNoon = Mathf.Abs(angle - 90f);
            t = Mathf.Clamp01(distFromNoon / 90f);
        }

        Color color = _gradient.Evaluate(t);
        Color ambientColor = _ambientGradient.Evaluate(t);

        skybox.SetColor("_Tint", color);
        RenderSettings.ambientSkyColor = ambientColor;

        DynamicGI.UpdateEnvironment();
    }

    // Time
    public void SetRotationFromT(float t)
    {
        t = Mathf.Repeat(t, 1f);

        float xAngle = t * 360f; 
        if (sky != null)
        {
            sky.transform.localRotation = Quaternion.Euler(xAngle, sunYRotation, 0f);
        }

        UpdateSun();
    }
}