Shader "Hidden/Muse/ShapePreview"
{
    Properties
    {
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;
            SamplerState my_linear_repeat_sampler;

            fixed4 _ReferenceColor;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = _MainTex.Sample(my_linear_repeat_sampler, i.uv);

                // interpolate between the original color and replacement color
                color.rgb = 1 - (1 - sqrt(sqrt(color.rgb))) * (1 - _ReferenceColor.rgb);

                #ifdef UNITY_COLORSPACE_GAMMA

                return color;

                #endif
                return fixed4(GammaToLinearSpace(color.rgb), color.a);
            }
            ENDCG
        }
    }
}
