using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class WeatherVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldManager worldManager;

    [Header("Slotted Weather Effects")]
    [SerializeReference] 
    private List<WeatherEffect> effects = new List<WeatherEffect>()
    {
        new RainWeatherEffect()
    };

    private WeatherType currentWeather = WeatherType.Clear;
    private float currentIntensity = 1.0f;
    private WeatherEffect activeEffect;

    void Awake()
    {
        if (effects == null || effects.Count == 0)
        {
            effects = new List<WeatherEffect>() { new RainWeatherEffect() };
        }
        ResolveWorldManager();
    }

    void OnEnable()
    {
        ResolveWorldManager();
        SubscribeWeather(); 
        SyncWeatherState(); 
        InitializeEffects();

        // Subscribe to URP render loop
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        UnsubscribeWeather();
        DisposeEffects();

        // Unsubscribe from URP render loop
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void ResolveWorldManager()
    {
        if (worldManager == null)
        {
            worldManager = WorldManager.Instance;
        }

        SyncWeatherState();
    }

    void SyncWeatherState()
    {
        if (worldManager != null && worldManager.Weather != null)
        {
            currentWeather = worldManager.Weather.CurrentWeather;
            currentIntensity = worldManager.Weather.WeatherIntensity;
            UpdateActiveEffectReference();
        }
    }

    void UpdateActiveEffectReference()
    {
        activeEffect = effects.Find(e => e != null && e.weatherType == currentWeather);
    }

    void SubscribeWeather()
    {
        UnsubscribeWeather();
        if (worldManager != null && worldManager.Weather != null)
        {
            worldManager.Weather.OnWeatherChanged += HandleWeatherChanged;
        }
    }

    void UnsubscribeWeather()
    {
        if (worldManager != null && worldManager.Weather != null)
        {
            worldManager.Weather.OnWeatherChanged -= HandleWeatherChanged;
        }
    }

    void HandleWeatherChanged(WeatherType newWeather, float intensity)
    {
        currentWeather = newWeather;
        currentIntensity = intensity;
        UpdateActiveEffectReference();
    }

    void InitializeEffects()
    {
        foreach (var effect in effects)
        {
            effect?.Initialize(transform);
        }
    }

    void DisposeEffects()
    {
        foreach (var effect in effects)
        {
            effect?.Dispose();
        }
    }

    void Update()
    {
        if (worldManager != null && worldManager.Weather != null)
        {
            currentWeather = worldManager.Weather.CurrentWeather;
            currentIntensity = worldManager.Weather.WeatherIntensity;
            UpdateActiveEffectReference();
        }

        activeEffect?.UpdateSimulation(currentIntensity, transform);
    }

    // Replaced legacy OnRenderObject with URP's explicit camera rendering callback
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView) return;
        if (activeEffect == null || currentIntensity <= 0f) return;

        activeEffect?.Render(currentIntensity, transform);
    }
}