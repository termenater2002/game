Shader "Hidden/MajorDamage/Pixelation"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            Name "Pixelate world"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _PixelationParams; // source width, height, square block width, height
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 size = _PixelationParams.xy;
                float2 block = _PixelationParams.zw;
                float2 uv = (floor(input.texcoord * size / block) * block + block * 0.5) / size;
                uv = clamp(uv, 0.5 / size, 1.0 - 0.5 / size);
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);
            }
            ENDHLSL
        }
    }
}
