Shader "MotionSynthesis/HeatmapShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent+1" }
        LOD 100
        Blend OneMinusDstColor One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            StructuredBuffer<float> bone_weights;
            StructuredBuffer<int> bone_index;
            StructuredBuffer<float> bone_distance;

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 col : COLOR0;
            };

            v2f vert (appdata v, uint vertexID : SV_VertexID)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                int index = bone_index[vertexID];
                float distance = 0;
                if (index >= 0)
                {
                    distance = bone_distance[index];
                }

                o.col.r = distance;
                o.col.g = 0;
                o.col.b = 0;
                o.col.a = distance;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.col;
                return col;
            }

            ENDCG
        }
        /*
        // Pass preventing editor selection
        // Non-ideal, breaks
        Pass
         {
             Name "Picking"
             Tags { "LightMode" = "Picking" }
             CGPROGRAM
                 #pragma vertex vert
                 #pragma fragment frag
                 #pragma multi_compile_instancing
                 #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
                 #include "UnityCG.cginc"
                 float4 vert(appdata_full v) : SV_POSITION
                 {
                     UNITY_SETUP_INSTANCE_ID(v);
                     float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                     return mul(UNITY_MATRIX_VP, worldPos);
                 }
                 uniform float4 _SelectionID;
                 fixed4 frag() : SV_Target
                 {
                     return _SelectionID;
                 }
             ENDCG
         }*/
    }
}
