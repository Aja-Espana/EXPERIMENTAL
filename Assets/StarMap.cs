using UnityEngine;

[System.Serializable]
public class StarMapData
{
    public int width;
    public int height;
    public Vector4[] data;

    public StarMapData(int width = 0, int height = 0)
    {
        this.width = width;
        this.height = height;

        int totalCells = width * height;
        this.data = new Vector4[totalCells];
    }
}

[ExecuteAlways]
public class StarMap : MonoBehaviour
{
    [Header("Material Target")]
    [SerializeField] private MeshRenderer skyMeshRenderer;

    [Header("Generated Outputs")]
    [SerializeField] public Texture2D starMapTexture;
    [SerializeField] private StarMapData starMapData;

    private Material materialInstance;
    private float t;

    private Color[] colorPalette = new Color[8]
    {
        new Color(1.0f, 0.2f, 0.1f), // Deep Red
        new Color(1.0f, 0.5f, 0.2f), // Orange
        new Color(1.0f, 0.9f, 0.4f), // Yellow
        new Color(1.0f, 1.0f, 0.9f), // Warm White
        new Color(0.9f, 0.95f, 1.0f),// Pure White
        new Color(0.6f, 0.8f, 1.0f), // Soft Blue
        new Color(0.3f, 0.6f, 1.0f), // Deep Blue
        new Color(0.2f, 0.4f, 1.0f)  // Intense Blue
    };

    private void Awake()
    {
        if (skyMeshRenderer == null) 
        {
            skyMeshRenderer = GetComponent<MeshRenderer>();
        }
            
        materialInstance = skyMeshRenderer.sharedMaterial;

        if (starMapTexture != null) 
        {
            ApplyToMaterial(materialInstance, starMapTexture);
        }
    }

    private void Update()
    {
        float alpha = 1f;

        if (t >= 0.1f && t <= 0.4f)
        {
            alpha = 0f;
        }
        else if (t < 0.1f)
        {
            alpha = Mathf.InverseLerp(0.1f, 0.0f, t);
        }
        else if (t > 0.4f && t < 0.5f)
        {
            alpha = Mathf.InverseLerp(0.4f, 0.5f, t);
        }
        else
        {
            alpha = 1f;
        }

        UpdateAlpha(alpha);
    }

    public void ApplyToMaterial(Material mat, Texture2D tex)
    {
        if (mat == null) return;

        if (tex != null)
        {
            mat.SetTexture("_StarDataMap", tex);
        }
        
        mat.SetInt("_GridWidth", tex.width);
        mat.SetInt("_GridHeight", tex.height);

        if (colorPalette != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i < colorPalette.Length)
                {
                    mat.SetColor("_Color" + i, colorPalette[i]);
                }
            }
        }
    }

    public void UpdateStarMapTexture(Texture2D texture)
    {
        starMapTexture = texture;
        if (starMapTexture != null) 
        {
            starMapData = TextureToStarMapData(starMapTexture);
            ApplyToMaterial(materialInstance, starMapTexture);
        }
    }

    public void UpdateAlpha(float alpha)
    {
        materialInstance.SetFloat("_AlphaMultiplier", alpha);
    }

    public void SetT(float t)
    {
        this.t = t;
    }

    // Star Map Data
    public Texture2D DataToStarMapTexture(StarMapData data) 
    {
        Texture2D texture = new Texture2D(data.width, data.height, TextureFormat.RGBAFloat, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                Vector4 star = data.data[index];

                if (star != Vector4.zero) 
                {
                    float localX   = star.x;
                    float localY   = star.y;
                    float colorVal = star.z / 7.0f;
                    float sizeVal  = star.w / 8.0f;

                    texture.SetPixel(x, y, new Color(localX, localY, colorVal, sizeVal));
                }
                else
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        texture.Apply();
        return texture;
    }

    public StarMapData TextureToStarMapData(Texture2D texture) 
    {
        StarMapData data = new StarMapData(texture.width, texture.height);
        var pixelData = texture.GetPixelData<Vector4>(0);
        data.data = pixelData.ToArray();

        return data;
    }
}