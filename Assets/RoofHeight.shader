Shader "Hidden/RoofHeightCapture"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            ZWrite On
            ZTest LEqual
            ColorMask R // We only need the red channel since texture is RFloat

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float worldY : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // Calculate absolute World Space Y coordinate
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldY = worldPos.y;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Output World Y into the R channel
                return float4(i.worldY, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}