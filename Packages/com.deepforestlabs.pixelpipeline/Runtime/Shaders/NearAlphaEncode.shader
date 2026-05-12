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

            float _PixelPipelineFadeStart;
            float _PixelPipelineFadeEnd;

            static const float Bayer4x4[16] =
            {
                 0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                 3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            };

            float BayerThreshold(uint2 pixel)
            {
                uint x = pixel.x & 3u;
                uint y = pixel.y & 3u;
                return Bayer4x4[y * 4u + x];
            }

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
