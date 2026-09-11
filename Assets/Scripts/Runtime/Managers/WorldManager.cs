using UnityEngine;
using UnityEngine.Events;
using System;

[ExecuteAlways]
public class WorldManager : MonoBehaviour
{
    public SkyboxManager skyboxManager;

    public enum TimeOfDay {
        Day,
        Night
    }

     public enum Direction {
        Forwards,
        Backwards
     }

    [SerializeField] StarMap starMap;
    [SerializeField] Texture2D starMapTexture;

    [Header("General")]
    [SerializeField] float timeScale = 1f;
    [SerializeField] float timeInDay = 600f;
    [SerializeField] float timeInNight = 600f;
    [Space(8)]
    [SerializeField] TimeOfDay timeOfDay = TimeOfDay.Day;
    [SerializeField] Direction direction = Direction.Forwards;
    [SerializeField] float currentTime = 0f;

    [Header("Weather")]
    [SerializeReference] WeatherController weather = new WeatherController();

    public WeatherController Weather => weather;

    // Events
    private Action<Texture2D> onStarMapUpdate;
    
    // Cache
    private Texture2D prevStarMapTexture;
    
    private static WorldManager instance;
    public static WorldManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<WorldManager>();
                if (instance == null)
                {
                    GameObject singleton = new GameObject(typeof(WorldManager).ToString());
                    instance = singleton.AddComponent<WorldManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (Application.isPlaying && GameContext.ctx != null)
        {
            starMapTexture = GameContext.ctx.starMapTexture;
        }
        
        if (starMap)
        {
            onStarMapUpdate += starMap.UpdateStarMapTexture;
        }
    }

    void Start()
    {
        weather.Initialize();
    }

    void Update()
    {
        UpdateData();

        weather.UpdateWeather();
    }

    void FixedUpdate()
    {
        if (Application.isPlaying) 
        {
            TickClock();
        }
    }

    void OnRenderObject()
    {
        weather.RenderWeather();
    }

    void OnEnable()
    {
        if (starMap)
        {
            onStarMapUpdate += starMap.UpdateStarMapTexture;
        }
    }

    void OnDisable()
    {
        onStarMapUpdate -= starMap.UpdateStarMapTexture;
    }

    void OnDestroy()
    {
        weather.Dispose();
    }

    void UpdateData()
    {
        // Star Map Texture
        if (starMapTexture != prevStarMapTexture) 
        {
            onStarMapUpdate?.Invoke(starMapTexture);
            prevStarMapTexture = starMapTexture;
        }
    }

    // Time
    void TickClock() 
    {
        float maxTime = timeInDay + timeInNight;
        int direction = this.direction == Direction.Forwards ? 1 : -1;
        currentTime += Time.fixedDeltaTime * timeScale * direction;
        timeOfDay = currentTime / maxTime < 0.5f ? TimeOfDay.Day : TimeOfDay.Night;

        float currentInterval = timeOfDay == TimeOfDay.Day ? timeInDay : timeInNight;
        if (currentTime > maxTime) currentTime = 0f;
        if (currentTime < 0f) currentTime = maxTime;
        
        float t = 0f;
        if (timeOfDay == TimeOfDay.Day)
        {
            t = (currentTime / timeInDay) * 0.5f; 
        }
        else if (timeOfDay == TimeOfDay.Night)
        {
            float adjustedTime = currentTime - timeInDay;
            t = 0.5f + ((adjustedTime / timeInNight) * 0.5f); 
        }

        skyboxManager?.SetRotationFromT(t);
        starMap?.SetT(t);
    }

    // Stars
    void UpdateStarMap()
    {
        
    }
}
