Shader "Custom/AdvancedPsychedelicMelt"
{
    Properties
    {
        _Intensity ("Trip Intensity", Range(0, 1)) = 1.0
        _MeltStrength ("Domain Warp Melt", Range(0, 0.15)) = 0.05
        _PixelScale ("Pixel Grid Scale", Range(32, 512)) = 180
        _StutterFPS ("Pixel Lag FPS", Range(4, 30)) = 12
        _PosterizeSteps ("RGB Posterization", Range(2, 16)) = 6
        _HueSpeed ("HSV Hue Speed", Float) = 0.4
        _BreathSpeed ("Radial Lens Pulse Speed", Float) = 1.5
        _Vibrance ("Color Vibrance", Range(0, 3)) = 1.0
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Contrast ("Contrast", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        LOD 100
        ZWrite Off 
        Cull Off 
        ZTest Always

        Pass
        {
            Name "AdvancedPsychedelicPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _MeltStrength;
            float _PixelScale;
            float _StutterFPS;
            float _PosterizeSteps;
            float _HueSpeed;
            float _BreathSpeed;
            float _Vibrance;
            float _Brightness;
            float _Contrast;

            // 1. Color Space Conversions (RGB <-> HSV)
            float3 RGB2HSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 HSV2RGB(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            // 2. Smooth 2D Noise
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // 3. Domain Warping
            float2 DomainWarp(float2 uv, float time, float strength)
            {
                float n1 = ValueNoise(uv * 3.5 + float2(time * 0.15, time * 0.1));
                float n2 = ValueNoise(uv * 3.5 + float2(-time * 0.1, time * 0.2));
                float2 warpedUV = uv + float2(n1, n2) * 0.2;

                float drip = ValueNoise(float2(warpedUV.x * 5.0, warpedUV.y * 1.8 - time * 0.35));
                
                float xOffset = (ValueNoise(uv * 8.0 + time * 0.5) - 0.5) * 0.03;
                float yOffset = drip;

                return float2(xOffset, -yOffset) * strength;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float intensity = saturate(_Intensity);
                float2 uv = input.texcoord;

                // Temporal Frame Stutter (Low-FPS Pixel Lag)
                float time = _Time.y;
                float lagTime = floor(time * _StutterFPS) / _StutterFPS;

                // Pixel Grid Snap
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 gridCount = float2(_PixelScale * aspect, _PixelScale);
                float2 pixelatedUV = floor(uv * gridCount) / gridCount;
                pixelatedUV = lerp(uv, pixelatedUV, intensity);

                // 4. Radial Lens "Breathing" / Fisheye Pumping
                float2 centerVec = (pixelatedUV - 0.5) * float2(aspect, 1.0);
                float distFromCenter = length(centerVec);
                float pulse = sin(lagTime * _BreathSpeed) * 0.25 * intensity;
                float2 breatheOffset = (pixelatedUV - 0.5) * pulse * distFromCenter;
                pixelatedUV += breatheOffset;

                // 5. Apply Domain Warped Liquid Dripping
                float2 meltOffset = DomainWarp(pixelatedUV, lagTime, _MeltStrength * intensity);
                float2 finalUV = pixelatedUV + meltOffset;

                // Chromatic Aberration Channel Sampling
                float abOffset = 0.015 * intensity * (1.0 + distFromCenter);
                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV + float2(abOffset, 0.0)).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV - float2(abOffset, 0.0)).b;
                float3 sceneColor = float3(r, g, b);

                // Cache distorted color to ensure absolute clean zero-baseline mapping
                float3 unadjustedColor = sceneColor;

                // 6. Luminance-Based Edge Silhouette Accentuation
                float3 centerSample = sceneColor;
                float3 rightSample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV + float2(0.003, 0.0)).rgb;
                float3 topSample   = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV + float2(0.0, 0.003)).rgb;
                float edgeDelta = length(centerSample - rightSample) + length(centerSample - topSample);
                float edgeMask = smoothstep(0.1, 0.4, edgeDelta);

                // 7. HSV Space Hue-Rotation, Vibrance & Brightness Control
                float3 hsv = RGB2HSV(sceneColor);

                float hueShift = lagTime * _HueSpeed + distFromCenter * 1.2 + meltOffset.y * 3.0;
                hsv.x = frac(hsv.x + hueShift * intensity);
                
                hsv.y = saturate(hsv.y * _Vibrance * (1.0 + intensity * 0.8) + edgeMask * 0.5 * intensity);
                hsv.z = saturate(hsv.z * _Brightness * (1.0 + intensity * 0.15));

                sceneColor = HSV2RGB(hsv);

                // Contrast Adjustment
                sceneColor = saturate((sceneColor - 0.5) * _Contrast + 0.5);

                // 8. RGB Posterization (Simplification)
                float steps = _PosterizeSteps;
                float3 posterized = floor(sceneColor * steps) / steps;
                sceneColor = lerp(sceneColor, posterized, intensity * 0.85);

                // Smoothly blend color operations back to exact unadjusted pixels based on intensity
                sceneColor = lerp(unadjustedColor, sceneColor, intensity);

                return float4(sceneColor, 1.0);
            }
            ENDHLSL
        }
    }
}