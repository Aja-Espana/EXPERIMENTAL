using UnityEngine;
using System;

[System.Serializable]
public abstract class WeatherEffect
{
    [Header("Effect Configuration")]
    public WeatherType weatherType;

    public abstract void Initialize(Transform transform);
    public abstract void UpdateSimulation(float intensity, Transform transform);
    public abstract void Render(float intensity, Transform transform);
    public abstract void Dispose();
}