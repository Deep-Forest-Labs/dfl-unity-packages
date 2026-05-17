Shader "DeepForestLabs/PixelPipeline/NearAlphaEncode"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        ColorMask A

        Pass
        {
            Name "NearAlphaEncode"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "BayerDither.hlsl"

            float _PixelPipelineFadeStart;
            float _PixelPipelineFadeEnd;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float rawDepth = SampleSceneDepth(input.texcoord);

                #if UNITY_REVERSED_Z
                    bool missedGeometry = rawDepth <= 0.0;
                #else
                    bool missedGeometry = rawDepth >= 1.0;
                #endif
                if (missedGeometry)
                {
                    return half4(0, 0, 0, 0);
                }

                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                float start = _PixelPipelineFadeStart;
                float end   = max(_PixelPipelineFadeEnd, start + 0.0001);
                float t     = saturate((eyeDepth - start) / (end - start));

                uint2 pixel = (uint2)(input.texcoord * _ScreenParams.xy);
                float threshold = BayerThreshold(pixel);

                float alpha = step(t, 1.0 - threshold);

                return half4(0, 0, 0, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
