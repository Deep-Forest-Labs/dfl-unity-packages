Shader "DeepForestLabs/PixelPipeline/Composite"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "PixelPipelineComposite"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_FarRT);
            TEXTURE2D(_NearRT);
            TEXTURE2D(_ViewmodelRT);

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 farCol  = SAMPLE_TEXTURE2D(_FarRT,       sampler_PointClamp, uv);
                half4 nearCol = SAMPLE_TEXTURE2D(_NearRT,      sampler_PointClamp, uv);
                half4 vmCol   = SAMPLE_TEXTURE2D(_ViewmodelRT, sampler_PointClamp, uv);

                half3 world = lerp(farCol.rgb, nearCol.rgb, nearCol.a);
                half3 rgb   = lerp(world, vmCol.rgb, vmCol.a);
                return half4(rgb, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
