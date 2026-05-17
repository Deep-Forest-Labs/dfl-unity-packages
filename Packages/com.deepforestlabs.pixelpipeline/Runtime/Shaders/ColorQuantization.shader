Shader "DeepForestLabs/PixelPipeline/ColorQuantization"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "ColorQuantization"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "BayerDither.hlsl"

            TEXTURE2D(_ViewmodelRT);

            float _ColorQuantizationLevels;
            float _QuantizeViewmodel;
            float _PixelPipelinePixelScale;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv).rgb;

                if (_ColorQuantizationLevels < 2.0)
                    return half4(color, 1.0);

                if (_QuantizeViewmodel < 0.5)
                {
                    half vmAlpha = SAMPLE_TEXTURE2D(_ViewmodelRT, sampler_PointClamp, uv).a;
                    if (vmAlpha > 0.5)
                        return half4(color, 1.0);
                }

                float scale = max(_PixelPipelinePixelScale, 1.0);
                uint2 pixel = (uint2)(uv * _ScreenParams.xy / scale);
                float bayerOffset = (BayerThreshold(pixel) - 0.5) / _ColorQuantizationLevels;

                half3 quantized = floor((color + bayerOffset) * _ColorQuantizationLevels + 0.5)
                                / _ColorQuantizationLevels;

                return half4(quantized, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
