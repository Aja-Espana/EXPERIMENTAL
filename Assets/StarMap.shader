Shader "Custom/StarMap"
{
    Properties
    {
        _StarDataMap ("Star Data Map", 2D) = "black" {}
        _GridWidth ("Grid Width", Int) = 128
        _GridHeight ("Grid Height", Int) = 64
        _AlphaMultiplier ("Alpha Multiplier", Range(0, 1)) = 1
        
        [HideInInspector] _Color0 ("Color 0", Color) = (1,1,1,1)
        [HideInInspector] _Color1 ("Color 1", Color) = (1,1,1,1)
        [HideInInspector] _Color2 ("Color 2", Color) = (1,1,1,1)
        [HideInInspector] _Color3 ("Color 3", Color) = (1,1,1,1)
        [HideInInspector] _Color4 ("Color 4", Color) = (1,1,1,1)
        [HideInInspector] _Color5 ("Color 5", Color) = (1,1,1,1)
        [HideInInspector] _Color6 ("Color 6", Color) = (1,1,1,1)
        [HideInInspector] _Color7 ("Color 7", Color) = (1,1,1,1)
    }

    SubShader
    {
        // Rendered right after the background cubemap, but before opaque geometry and planetary bodies
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent-2" 
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 localPos   : TEXCOORD0; 
            };

            TEXTURE2D(_StarDataMap);
            SAMPLER(sampler_StarDataMap);

            CBUFFER_START(UnityPerMaterial)
                int _GridWidth;
                int _GridHeight;
                float _AlphaMultiplier;
                float4 _Color0, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings output;
                float3 worldPos = _WorldSpaceCameraPos + TransformObjectToWorldDir(IN.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);

                #if defined(UNITY_REVERSED_Z)
                output.positionCS.z = 1.0e-9f;
                #else
                output.positionCS.z = output.positionCS.w - 1.0e-6f;
                #endif

                output.localPos = IN.positionOS.xyz;
                
                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.localPos);
                float3 absDir = abs(dir);
                float2 gridSpace;

                if (absDir.x >= absDir.y && absDir.x >= absDir.z)
                    gridSpace = float2((dir.x > 0.0 ? -dir.z : dir.z), dir.y) / absDir.x;
                else if (absDir.y >= absDir.x && absDir.y >= absDir.z)
                    gridSpace = float2(dir.x, (dir.y > 0.0 ? -dir.z : dir.z)) / absDir.y;
                else
                    gridSpace = float2((dir.z > 0.0 ? dir.x : -dir.x), dir.y) / absDir.z;

                gridSpace = (gridSpace * 0.5 + 0.5) * float2(_GridWidth, _GridHeight);
                int2 currentCell = int2(floor(gridSpace));

                float4 colorPalette[8] = { _Color0, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7 };

                float3 accumulatedColor = float3(0, 0, 0);
                float accumulatedAlpha = 0.0;

                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        int2 evalCell = currentCell + int2(xOffset, yOffset);
                        evalCell.x = clamp(evalCell.x, 0, _GridWidth - 1);
                        evalCell.y = clamp(evalCell.y, 0, _GridHeight - 1);

                        float4 starData = LOAD_TEXTURE2D(_StarDataMap, evalCell);
                        float starRadius = starData.a * 0.25f; 
                        if (starRadius <= 0.001f) continue;

                        float2 starCenter = float2(evalCell) + starData.rg;
                        float intensity = smoothstep(starRadius, 0.0f, length(gridSpace - starCenter));
                        
                        if (intensity > 0.0f)
                        {
                            int colorIndex = clamp((int)round(starData.b * 7.0f), 0, 7);
                            accumulatedColor += colorPalette[colorIndex].rgb * intensity;
                            accumulatedColor *= _AlphaMultiplier;
                        }
                    }
                }

                return half4(accumulatedColor, accumulatedAlpha);
            }
            ENDHLSL
        }
    }
}