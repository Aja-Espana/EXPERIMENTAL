using UnityEngine;
using System;

public enum WeatherType
{
    Clear,
    Rain,
    Snow,
    Fog
}

[System.Serializable]
public class WeatherController
{
    [Header("Current Weather State")]
    [SerializeField] private WeatherType currentWeather = WeatherType.Rain;
    [Range(0f, 2f)] [SerializeField] private float weatherIntensity = 1.0f;

    public event Action<WeatherType, float> OnWeatherChanged;

    public WeatherType CurrentWeather => currentWeather;
    public float WeatherIntensity => weatherIntensity;

    public void SetWeather(WeatherType newWeather, float intensity = 1.0f)
    {
        currentWeather = newWeather;
        weatherIntensity = Mathf.Clamp(intensity, 0f, 2f);
        
        Debug.Log($"[WeatherController] SetWeather called: {currentWeather} with intensity {weatherIntensity}. Subscribers count: {OnWeatherChanged?.GetInvocationList().Length ?? 0}");
        
        OnWeatherChanged?.Invoke(currentWeather, weatherIntensity);
    }

    // WorldManager calls these based on your script, 
    // so we keep them as clean stubs or state updaters.
    public void Initialize()
    {
        // Global state initialization if needed
    }

    public void UpdateWeather()
    {
        // If networking is added later, server-side weather logic goes here.
        // Client rendering is entirely handled by the player's WeatherVisualizer.
    }

    public void RenderWeather()
    {
        // Rendering has been delegated to the player's WeatherVisualizer.
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}