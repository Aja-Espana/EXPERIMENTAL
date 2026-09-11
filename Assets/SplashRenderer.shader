Shader "Custom/SplashRenderer"
{
    Properties
    {
        _SplashTex("Splash Sprite / Texture", 2D) = "white" {}
        _SplashColor("Splash Tint Color", Color) = (0.7, 0.8, 1.0, 0.6)
        _MaxSplashSize("Max Ripple Size", Float) = 0.5
        _RainIntensity("Rain Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+50" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertSplash
            #pragma fragment FragSplash
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct SplashParticle
            {
                float4 posAndLife;   // xyz = position, w = current lifetime
                float4 metaAndPad;   // x = maxLifetime, yzw = padding
            };

            StructuredBuffer<SplashParticle> _SplashBuffer;
            TEXTURE2D(_SplashTex);
            SAMPLER(sampler_SplashTex);
            float4 _SplashColor;
            float _MaxSplashSize;
            float _RainIntensity;

            struct v2f_splash
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float progress : TEXCOORD1;
            };

            v2f_splash VertSplash(uint vertexID : SV_VertexID)
            {
                uint id = vertexID / 6;
                uint vIndex = vertexID % 6;
                SplashParticle s = _SplashBuffer[id];

                float lifetime = s.posAndLife.w;
                float maxLifetime = s.metaAndPad.x;

                // Safely discard dead or uninitialized splashes to prevent NaN/corruption explosions
                if (lifetime <= 0.0f || maxLifetime <= 0.0f)
                {
                    v2f_splash deadOut;
                    deadOut.positionCS = float4(0.0, 0.0, -1000.0, 1.0);
                    deadOut.uv = float2(0, 0);
                    deadOut.progress = 1.0;
                    return deadOut;
                }

                float2 offsets[6] = {
                    float2(-0.5,  0.5), float2( 0.5,  0.5), float2( 0.5, -0.5), 
                    float2(-0.5,  0.5), float2( 0.5, -0.5), float2(-0.5, -0.5)
                };
                float2 uvs[6] = {
                    float2(0, 1), float2(1, 1), float2(1, 0),
                    float2(0, 1), float2(1, 0), float2(0, 0)
                };

                float progress = saturate(1.0 - (lifetime / maxLifetime));
                float currentSize = lerp(0.05, _MaxSplashSize, progress);

                float3 camRight = UNITY_MATRIX_V[0].xyz;
                float3 camUp    = UNITY_MATRIX_V[1].xyz;
                float3 worldPos = s.posAndLife.xyz + camRight * offsets[vIndex].x * currentSize + camUp * offsets[vIndex].y * currentSize;

                v2f_splash o;
                o.positionCS = TransformWorldToHClip(worldPos);
                o.uv = uvs[vIndex];
                o.progress = progress;
                return o;
            }

            float4 FragSplash(v2f_splash input) : SV_Target
            {
                if (input.progress >= 1.0f) discard;
                
                float4 tex = SAMPLE_TEXTURE2D(_SplashTex, sampler_SplashTex, input.uv);
                float alphaFade = 1.0 - input.progress;
                return float4(_SplashColor.rgb, tex.a * _SplashColor.a * alphaFade * _RainIntensity);
            }
            ENDHLSL
        }
    }
}