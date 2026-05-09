Shader "UI/Unlit/Alpha8Vertex"
{
    Properties
    {
        _MainTex        ("Alpha Mask (RGBA)", 2D) = "white" {}
        _Color          ("Tint Color",     Color) = (1,1,1,1)

        [HideInInspector] _Stencil           ("Stencil Ref",        Float) = 0
        [HideInInspector] _StencilOp         ("Stencil Op",         Float) = 0
        [HideInInspector] _StencilComp       ("Stencil Comp",       Float) = 8
        [HideInInspector] _StencilReadMask   ("Stencil Read Mask",  Float) = 255
        [HideInInspector] _StencilWriteMask  ("Stencil Write Mask", Float) = 255
        [HideInInspector] _ColorMask         ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"            = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas"= "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]

        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            fixed4    _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.texcoord;
                o.color  = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed alpha = tex2D(_MainTex, i.uv).a * i.color.a;
                clip(alpha - 0.001);
                return fixed4(i.color.rgb, alpha);
            }
            ENDCG
        }
    }
}
