Shader "ng/Unlit/Painting"
{
    Properties
    {
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"{}
        _Radius ("Radius", Range(0.0, 1.0)) = 0.01
        _PaintPosX ("Paint position X", Range(0.0, 1.0)) = 0.0
        _PaintPosY ("Paint position Y", Range(0.0, 1.0)) = 0.0
        [MaterialToggle] _IsSeamless("Is seamless", float) = 0.
        [MaterialToggle] _WrapAround("Wrap around", float) = 1.
        _EdgeMargin ("Seamless margin", Range(0.0, 1.0)) = 0.03125
        [MaterialToggle] _IsErasing("Is erasing", float) = 0.
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        LOD 100

        ZWrite Off
        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _Radius;
            float _PaintPosX;
            float _PaintPosY;
            float _IsErasing;
            float _IsSeamless;
            float _EdgeMargin; // Adjust this according to your texture size.
            float _WrapAround;

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
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 paintPos = float2(_PaintPosX, _PaintPosY);
                float2 distVec = abs(i.uv - paintPos);
                if (_WrapAround)
                    distVec = min(distVec, 1.0 - distVec); // wrap-around
                float dist = length(distVec);

                const float alphaEdge = i.uv.x < _EdgeMargin || i.uv.y < _EdgeMargin || i.uv.x > 1.0 - _EdgeMargin || i.
                    uv.y > 1.0 - _EdgeMargin;

                // no need to calculate if _IsSeamless == 0
                if (_IsSeamless == 1. && alphaEdge)
                {
                    col.a = 0;
                }
                else if (dist <= _Radius)
                {
                    col.a = 1. - _IsErasing;
                }

                return col;
            }
            ENDCG
        }
    }
}