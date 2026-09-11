Shader "Custom/RainRenderer"
{
    Properties
    {
        _RainColor("Rain Color", Color) = (0.7, 0.8, 1.0, 0.4)
        _RainLength("Rain Length", Float) = 0.5
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+50" 
            "RenderPipeline"="UniversalPipeline" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct RainParticle {
                float3 position;
                float pad1;
                float3 velocity;
                float pad2;
                float lifetime;
                float3 pad3;
            };

            StructuredBuffer<RainParticle> _ParticleBuffer;

            CBUFFER_START(UnityPerMaterial)
                float4 _RainColor;
                float _RainLength;
            CBUFFER_END

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(uint vertexID : SV_VertexID)
            {
                uint particleID = vertexID / 6;
                uint vertIndex = vertexID % 6;

                RainParticle p = _ParticleBuffer[particleID];
                
                float3 dir = normalize(p.velocity);
                
                // Calculate a width vector that always faces the camera
                float3 viewDir = normalize(_WorldSpaceCameraPos - p.position);
                float3 widthDir = normalize(cross(dir, viewDir));
                float3 thickness = widthDir * 0.05;

                float3 offsets[6] = {
                    float3(0,0,0), 
                    dir * _RainLength, 
                    dir * _RainLength + thickness,
                    float3(0,0,0), 
                    dir * _RainLength + thickness, 
                    thickness
                };

                float3 worldPos = p.position + offsets[vertIndex];

                v2f o;
                o.pos = TransformWorldToHClip(worldPos);
                o.color = _RainColor;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}