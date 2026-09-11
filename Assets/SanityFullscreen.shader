Shader "Custom/AdvancedHorrorSanity"
{
    Properties
    {
        _Sanity ("Sanity (1 = Sane, 0 = Insane)", Range(0, 1)) = 1.0
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _VignetteSize ("Vignette Size", Range(0.2, 2.0)) = 1.0
        _VignetteDensity ("Vignette Density", Float) = 1.0
        _DistortionIntensity ("Distortion Strength", Float) = 0.02
        _AberrationStrength ("Chromatic Aberration", Float) = 0.03
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
            Name "SanityPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Sanity;
            float _PulseSpeed;
            float _VignetteSize;
            float _VignetteDensity;
            float _DistortionIntensity;
            float _AberrationStrength;

            // Pseudo-random noise
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Smooth 2D noise
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

            float4 Frag(Varyings input) : SV_Target
            {
                float insanity = 1.0 - saturate(_Sanity);
                float2 uv = input.texcoord;

                // Early exit if completely sane
                if (insanity <= 0.001)
                {
                    return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                }

                float time = _Time.y;

                // 1. Heartbeat Pulse Calculation
                float pulse = (sin(time * _PulseSpeed) * 0.5 + 0.5) * 
                              (sin(time * _PulseSpeed * 2.0) * 0.3 + 0.7);
                float pulseScale = lerp(1.0, 1.25, pulse * insanity);

                // 2. Screen Wobble & Micro-Glitches
                float glitch = step(0.95, Hash21(float2(floor(time * 12.0), 0.0))) * step(_Sanity, 0.3);
                float2 offset;
                offset.x = sin(time * 3.0 + uv.y * 12.0) * _DistortionIntensity;
                offset.y = cos(time * 2.5 + uv.x * 10.0) * _DistortionIntensity * 0.5;
                offset += (Hash21(uv + time) - 0.5) * 0.04 * glitch;

                float2 distortedUV = uv + offset * insanity;

                // Aspect ratio correction
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 aspectUV = (distortedUV - 0.5) * float2(aspect, 1.0);
                float distFromCenter = length(aspectUV);

                // 3. Chromatic Aberration Sampling
                float abAmount = _AberrationStrength * insanity * (1.0 + distFromCenter);
                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV + float2(abAmount, 0.0)).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV - float2(abAmount, 0.0)).b;
                float4 sceneColor = float4(r, g, b, 1.0);

                // 4. Color Grading
                float lum = dot(sceneColor.rgb, float3(0.2126, 0.7152, 0.0722));
                float3 highContrast = smoothstep(0.1, 0.9, sceneColor.rgb);
                float3 horrorTint = lerp(float3(lum * 1.4, lum * 0.2, lum * 0.1), 
                                         float3(lum * 0.3, lum * 0.4, lum * 0.2), 
                                         sin(time) * 0.5 + 0.5);

                sceneColor.rgb = lerp(sceneColor.rgb, highContrast, insanity * 0.35);
                sceneColor.rgb = lerp(sceneColor.rgb, horrorTint, insanity * 0.6);

                // 5. ADJUSTABLE VIGNETTE SIZE & RADIUS
                float edgeNoise = (ValueNoise(uv * 4.0 + float2(time * 0.08, time * 0.05)) - 0.5) * 0.06 * insanity;

                // Scaled by _VignetteSize multiplier
                float innerRadius = lerp(0.65, 0.12 / pulseScale, insanity) * _VignetteSize;
                float outerRadius = lerp(1.2, 0.65, insanity) * _VignetteSize;

                // Smooth feathering & power falloff
                float vignetteFactor = smoothstep(outerRadius + edgeNoise, innerRadius + edgeNoise, distFromCenter);
                vignetteFactor = pow(saturate(vignetteFactor), 1.8);

                sceneColor.rgb *= lerp(1.0, vignetteFactor, insanity * _VignetteDensity);

                // 6. Sensor Grain
                float grain = (Hash21(uv * _ScreenParams.xy + frac(time)) - 0.5) * 0.15 * insanity;
                sceneColor.rgb += grain;

                return sceneColor;
            }
            ENDHLSL
        }
    }
}