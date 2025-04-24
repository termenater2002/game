Shader "Hidden/DoodlePad"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "black" {}
        _PatternTex("Pattern Texture", 2D) = "white" {}
        _MaskColor("Mask Color", Color) = (1, 1, 1, 1)
        _PatternRepeat("Pattern Repeat", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

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
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
                float2 clipUV : TEXCOORD1;
                float2 screenpos : TEXCOORD2;
            };

            uniform float4 _MainTex_ST;
            uniform float4x4 unity_GUIClipTextureMatrix;
            sampler2D _GUIClipTexture;
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;
                float3 eyePos = UnityObjectToViewPos(IN.vertex);
                OUT.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
                OUT.screenpos = ComputeScreenPos(OUT.vertex);

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _PatternTex;

            float4 _MainTex_TexelSize;

            float4 _MaskColor;

            float _PatternRepeat;

            fixed4 frag(v2f i) : SV_Target
            {
                if (tex2D(_GUIClipTexture, i.clipUV).a == 0)
                    discard;
                const float4 patternMask = tex2D(_PatternTex, frac(i.screenpos * _PatternRepeat));
                float4 col = tex2D(_MainTex, i.texcoord) * patternMask * _MaskColor;
                return col;
            }
            ENDCG
        }
    }
}