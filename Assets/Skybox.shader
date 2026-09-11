Shader "Custom/Skybox"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos + _WorldSpaceCameraPos);
                output.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                output.positionCS.z = 0.00001 * output.positionCS.w;
                
                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return texColor;
            }
            ENDHLSL
        }
    }
}
