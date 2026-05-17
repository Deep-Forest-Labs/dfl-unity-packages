Shader "DeepForestLabs/PixelPipeline/FarFog"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "FarFog"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "BayerDither.hlsl"

            float _FarFogEnabled;
            float _FarFogStart;
            float _FarFogEnd;
            half4 _FarFogColorLight;
            half4 _FarFogColorDark;

            float _FarColorShiftEnabled;
            float _FarColorShiftStart;
            float _FarColorShiftEnd;
            half4 _FarColorShiftTint;
            float _FarColorShiftDesaturation;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv).rgb;

                float rawDepth = SampleSceneDepth(uv);

                #if UNITY_REVERSED_Z
                    bool missedGeometry = rawDepth <= 0.0;
                #else
                    bool missedGeometry = rawDepth >= 1.0;
                #endif
                if (missedGeometry)
                {
                    return half4(color, 1.0);
                }

                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                uint2 pixel = (uint2)(uv * _ScreenParams.xy);
                float threshold = BayerThreshold(pixel);

                if (_FarColorShiftEnabled > 0.5)
                {
                    float shiftStart = _FarColorShiftStart;
                    float shiftEnd = max(_FarColorShiftEnd, shiftStart + 0.001);
                    float shiftT = saturate((eyeDepth - shiftStart) / (shiftEnd - shiftStart));

                    half lum = dot(color, half3(0.2126, 0.7152, 0.0722));
                    half3 gray = half3(lum, lum, lum);
                    half3 desaturated = lerp(color, gray, _FarColorShiftDesaturation);
                    color = lerp(color, desaturated * _FarColorShiftTint.rgb, shiftT);
                }

                if (_FarFogEnabled > 0.5)
                {
                    float fogStart = _FarFogStart;
                    float fogEnd = max(_FarFogEnd, fogStart + 0.001);
                    float fogT = saturate((eyeDepth - fogStart) / (fogEnd - fogStart));

                    half3 fogColor = lerp(_FarFogColorLight.rgb, _FarFogColorDark.rgb, threshold);
                    color = fogT > threshold ? fogColor : color;
                }

                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
