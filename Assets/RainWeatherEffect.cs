using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

[System.Serializable]
public class RainWeatherEffect : WeatherEffect
{
    [Header("Compute & Materials")]
    public ComputeShader rainCompute;
    public Material rainMaterial;
    public Material splashMaterial;

    [Header("Simulation Settings")]
    public int particleCount = 10000;
    public Vector3 boundsMin = new Vector3(-50, 0, -50);
    public Vector3 boundsMax = new Vector3(50, 25, 50);
    public float groundY = 0;
    public LayerMask roofLayer;
    public float scanInterval = 0.2f;
    public float splashLifetime = 0.6f;

    [Header("Audio Clips")]
    public AudioClip ambientRainClip;      // Noise/ambient rain loop (fades in only at intensity >= 0.5)
    public AudioClip closeRainLoopClip;    // Close-by rain loop (scales immediately from intensity > 0)

    [Header("Audio Settings")]
    public bool enableRainAudio = true;
    [Range(0f, 3f)] public float ambientAudioVolume = 1.0f; // Independent volume for ambient noise
    [Range(0f, 3f)] public float closeLoopVolume = 1.0f;   // Independent volume for close-by loop

    [Header("Indoor / Low-Pass Filter Settings")]
    [Range(0f, 1f)] public float indoorFactor = 0f; // 0 = fully outside, 0.5 = 800Hz cutoff, 0.75 = no detail, 1 = nothing
    public float outdoorCutoff = 22000f; 
    public float filterTransitionSpeed = 4f;

    private ComputeBuffer particleBuffer, roofBoxBuffer, splashBuffer;
    private int rainKernel;
    private float scanTimer;
    private Collider[] overlapResults = new Collider[32];

    // Audio sources & filters
    private AudioSource ambientAudioSource;
    private AudioSource closeLoopAudioSource;
    private AudioLowPassFilter ambientLowPass;
    private AudioLowPassFilter closeLowPass;

    [StructLayout(LayoutKind.Sequential)]
    struct RainParticle
    {
        public Vector3 position;
        public float pad1;
        public Vector3 velocity;
        public float pad2;
        public float lifetime;
        public Vector3 pad3;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SplashParticle
    {
        public Vector4 posAndLife;
        public Vector4 metaAndPad;
    }

    struct RoofBoxData 
    { 
        public Vector4 minBoundsPad; 
        public Vector4 maxBoundsPad; 
    }

    public RainWeatherEffect()
    {
        weatherType = WeatherType.Rain;
    }

    public override void Initialize(Transform transform)
    {
        if (rainCompute == null)
        {
            Debug.LogError("[RainWeatherEffect] rainCompute is NULL! Inspector reference unassigned.");
            return;
        }

        rainKernel = rainCompute.FindKernel("CSMain");
        
        if (particleBuffer == null || particleBuffer.count != particleCount)
        {
            particleBuffer?.Release();
            particleBuffer = new ComputeBuffer(particleCount, 48);

            var data = new RainParticle[particleCount];
            Vector3 min = transform.position + boundsMin;
            Vector3 max = transform.position + boundsMax;

            for (int i = 0; i < particleCount; i++)
            {
                float x = UnityEngine.Random.Range(min.x, max.x);
                float y = UnityEngine.Random.Range(min.y, max.y);
                float z = UnityEngine.Random.Range(min.z, max.z);
                data[i] = new RainParticle
                {
                    position = new Vector3(x, y, z),
                    pad1 = 0f,
                    velocity = new Vector3(0f, -25f, 0f),
                    pad2 = 0f,
                    lifetime = UnityEngine.Random.Range(2f, 6f),
                    pad3 = Vector3.zero
                };
            }
            particleBuffer.SetData(data);
        }

        if (splashBuffer == null || splashBuffer.count != particleCount)
        {
            splashBuffer?.Release();
            splashBuffer = new ComputeBuffer(particleCount, 32);

            var splashData = new SplashParticle[particleCount];
            for (int i = 0; i < particleCount; i++)
                splashData[i] = new SplashParticle { posAndLife = Vector4.zero, metaAndPad = Vector4.zero };
            splashBuffer.SetData(splashData);
        }

        rainCompute.SetBuffer(rainKernel, "_ParticleBuffer", particleBuffer);
        rainCompute.SetBuffer(rainKernel, "_SplashBuffer", splashBuffer);
        rainCompute.SetInt("_ParticleCount", particleCount);
        rainCompute.SetFloat("_SplashLifetime", splashLifetime);

        UpdateRoofs(transform);
        InitializeAudioSources(transform);
    }

    private void InitializeAudioSources(Transform transform)
    {
        if (!enableRainAudio) return;

        // 1. Ambient Noise Audio Source
        ambientAudioSource = transform.GetComponent<AudioSource>();
        if (ambientAudioSource == null)
        {
            ambientAudioSource = transform.gameObject.AddComponent<AudioSource>();
        }
        ambientAudioSource.clip = ambientRainClip;
        ambientAudioSource.loop = true;
        ambientAudioSource.spatialBlend = 0.25f; 
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.volume = 0f;
        if (ambientRainClip != null) ambientAudioSource.Play();

        ambientLowPass = ambientAudioSource.GetComponent<AudioLowPassFilter>();
        if (ambientLowPass == null) ambientLowPass = ambientAudioSource.gameObject.AddComponent<AudioLowPassFilter>();
        ambientLowPass.cutoffFrequency = outdoorCutoff;

        // 2. Close-by Rain Loop Audio Source (Child Object for Spatialization)
        Transform closeObjTransform = transform.Find("CloseRainLoopSource");
        GameObject closeObj;
        if (closeObjTransform == null)
        {
            closeObj = new GameObject("CloseRainLoopSource");
            closeObj.transform.SetParent(transform);
            closeObj.transform.localPosition = Vector3.zero;
        }
        else
        {
            closeObj = closeObjTransform.gameObject;
        }

        closeLoopAudioSource = closeObj.GetComponent<AudioSource>();
        if (closeLoopAudioSource == null)
        {
            closeLoopAudioSource = closeObj.AddComponent<AudioSource>();
        }
        closeLoopAudioSource.clip = closeRainLoopClip;
        closeLoopAudioSource.loop = true;
        closeLoopAudioSource.spatialBlend = 0.25f; 
        closeLoopAudioSource.playOnAwake = false;
        closeLoopAudioSource.volume = 0f;
        if (closeRainLoopClip != null) closeLoopAudioSource.Play();

        closeLowPass = closeLoopAudioSource.GetComponent<AudioLowPassFilter>();
        if (closeLowPass == null) closeLowPass = closeLoopAudioSource.gameObject.AddComponent<AudioLowPassFilter>();
        closeLowPass.cutoffFrequency = outdoorCutoff;
    }

    public override void UpdateSimulation(float intensity, Transform transform)
    {
        if (enableRainAudio)
        {
            float indoorMult = Mathf.Clamp01(indoorFactor);

            // Piecewise cutoff frequency mapping based on your precise steps:
            // 0.0 -> outdoorCutoff (unaffected)
            // 0.5 -> 800Hz
            // 0.75 -> 150Hz (no detail)
            // 1.0 -> 10Hz (nothing)
            float targetCutoff;
            if (indoorMult <= 0.5f)
            {
                targetCutoff = Mathf.Lerp(outdoorCutoff, 800f, indoorMult / 0.5f);
            }
            else if (indoorMult <= 0.75f)
            {
                targetCutoff = Mathf.Lerp(800f, 150f, (indoorMult - 0.5f) / 0.25f);
            }
            else
            {
                targetCutoff = Mathf.Lerp(150f, 10f, (indoorMult - 0.75f) / 0.25f);
            }

            // Volume multiplier that scales down to 0 as indoorFactor approaches 1
            float indoorVolumeMultiplier = 1f;
            if (indoorMult > 0.5f)
            {
                indoorVolumeMultiplier = 1f - Mathf.Clamp01((indoorMult - 0.5f) / 0.5f);
            }

            if (ambientLowPass != null)
            {
                ambientLowPass.cutoffFrequency = Mathf.Lerp(ambientLowPass.cutoffFrequency, targetCutoff, Time.deltaTime * filterTransitionSpeed);
            }
            if (closeLowPass != null)
            {
                closeLowPass.cutoffFrequency = Mathf.Lerp(closeLowPass.cutoffFrequency, targetCutoff, Time.deltaTime * filterTransitionSpeed);
            }

            // Ambient Volume Modulation
            if (ambientAudioSource != null && ambientRainClip != null)
            {
                float targetAmbientVol = 0f;
                if (intensity >= 0.5f)
                {
                    float ambientIntensityAlpha = Mathf.Clamp01((intensity - 0.5f) / 1.5f); 
                    targetAmbientVol = ambientIntensityAlpha * ambientAudioVolume * indoorVolumeMultiplier;
                }
                
                ambientAudioSource.volume = Mathf.MoveTowards(ambientAudioSource.volume, targetAmbientVol, Time.deltaTime * 2f);
                ambientAudioSource.pitch = Mathf.Lerp(0.85f, 1.15f, Mathf.Clamp01(intensity / 2f));
            }

            // Close-by Volume Modulation
            if (closeLoopAudioSource != null && closeRainLoopClip != null)
            {
                if (Camera.main != null)
                {
                    closeLoopAudioSource.transform.position = Camera.main.transform.position;
                }

                float closeIntensityAlpha = Mathf.Clamp01(intensity / 2f); 
                float targetCloseVol = closeIntensityAlpha * closeLoopVolume * indoorVolumeMultiplier;
                closeLoopAudioSource.volume = Mathf.MoveTowards(closeLoopAudioSource.volume, targetCloseVol, Time.deltaTime * 2f);
            }
        }

        if (rainCompute == null || intensity <= 0f) return;

        scanTimer += Time.deltaTime;
        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            UpdateRoofs(transform);
        }

        rainCompute.SetFloat("_DeltaTime", Time.deltaTime);
        rainCompute.SetInt("_FrameSeed", Time.frameCount);
        rainCompute.SetFloat("_RainIntensity", intensity);
        rainCompute.SetVector("_BoundsMin", transform.position + boundsMin);
        rainCompute.SetVector("_BoundsMax", transform.position + boundsMax);
        rainCompute.SetFloat("_GroundY", transform.position.y + groundY);
        rainCompute.SetFloat("_SplashLifetime", splashLifetime);

        if (roofBoxBuffer != null)
        {
            rainCompute.SetBuffer(rainKernel, "_RoofBoxes", roofBoxBuffer);
        }

        float intensityFactor = Mathf.Clamp01(intensity / 2f);
        int activeParticlesToProcess = Mathf.CeilToInt(particleCount * intensityFactor);
        int threadGroups = Mathf.CeilToInt(activeParticlesToProcess / 256f);

        rainCompute.Dispatch(rainKernel, threadGroups, 1, 1);
    }

    public override void Render(float intensity, Transform transform)
    {
        if (intensity <= 0f) return;
        
        float intensityFactor = Mathf.Clamp01(intensity / 2f);
        int activeParticlesToProcess = Mathf.CeilToInt(particleCount * intensityFactor);

        Vector3 renderMin = new Vector3(boundsMin.x, boundsMin.y - 0.6f, boundsMin.z);
        Vector3 renderMax = boundsMax;
        Vector3 boxCenter = transform.position + (renderMin + renderMax) * 0.5f;
        Bounds worldBounds = new Bounds(boxCenter, renderMax - renderMin);

        if (rainMaterial != null && particleBuffer != null)
        {
            MaterialPropertyBlock rainProps = new MaterialPropertyBlock();
            rainProps.SetBuffer("_ParticleBuffer", particleBuffer);
            rainProps.SetFloat("_RainIntensity", intensity);

            Graphics.RenderPrimitives(new RenderParams(rainMaterial)
            {
                worldBounds = worldBounds,
                matProps = rainProps,
            }, MeshTopology.Triangles, activeParticlesToProcess * 6, 1);
        }

        if (splashMaterial != null && splashBuffer != null)
        {
            MaterialPropertyBlock splashProps = new MaterialPropertyBlock();
            splashProps.SetBuffer("_SplashBuffer", splashBuffer);
            splashProps.SetFloat("_RainIntensity", intensity);

            Graphics.RenderPrimitives(new RenderParams(splashMaterial)
            {
                worldBounds = worldBounds,
                matProps = splashProps
            }, MeshTopology.Triangles, activeParticlesToProcess * 6, 1);
        }
    }

    public override void Dispose()
    {
        particleBuffer?.Release();
        particleBuffer = null;
        roofBoxBuffer?.Release();
        roofBoxBuffer = null;
        splashBuffer?.Release();
        splashBuffer = null;

        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }

        if (closeLoopAudioSource != null)
        {
            closeLoopAudioSource.Stop();
        }
    }

    private void UpdateRoofs(Transform transform)
    {
        if (rainCompute == null) return;

        Vector3 center = transform.position + (boundsMin + boundsMax) * 0.5f;
        int hitCount = Physics.OverlapBoxNonAlloc(center, (boundsMax - boundsMin) * 0.5f, overlapResults, transform.rotation, roofLayer);

        if (hitCount > 0)
        {
            RoofBoxData[] boxes = new RoofBoxData[hitCount];
            for (int i = 0; i < hitCount; i++)
            {
                Bounds b = overlapResults[i].bounds;
                boxes[i] = new RoofBoxData
                {
                    minBoundsPad = new Vector4(b.min.x, b.min.y, b.min.z, 0),
                    maxBoundsPad = new Vector4(b.max.x, b.max.y, b.max.z, 0)
                };
            }

            if (roofBoxBuffer == null || roofBoxBuffer.count != hitCount)
            {
                roofBoxBuffer?.Release();
                roofBoxBuffer = new ComputeBuffer(hitCount, sizeof(float) * 8);
            }
            roofBoxBuffer.SetData(boxes);
        }
        else
        {
            if (roofBoxBuffer == null || roofBoxBuffer.count != 1)
            {
                roofBoxBuffer?.Release();
                roofBoxBuffer = new ComputeBuffer(1, sizeof(float) * 8);
                roofBoxBuffer.SetData(new RoofBoxData[] { new RoofBoxData() });
            }
        }

        rainCompute.SetBuffer(rainKernel, "_RoofBoxes", roofBoxBuffer);
        rainCompute.SetInt("_RoofBoxCount", hitCount);
    }
}