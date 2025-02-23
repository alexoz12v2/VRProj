Shader"Unlit/SimpleFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor("Fog Color", Color) = (1, 1, 1, 1)
        _FogDensity("Fog Density", Range(0, 1)) = 0.2
        _FogOffset("Fog Offset", Range(0, 100)) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _FogColor;
            float _FogDensity, _FogOffset;

            struct VertexData
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (VertexData IN)
            {
                Varyings OUT;
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                int x, y;
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float depth = SampleSceneDepth(IN.uv);
                //depth = Linear01Depth(depth);
                
                float viewDistance = depth * _ProjectionParams.z;
    
                float fogFactor = (_FogDensity / sqrt(log(2))) * max(0.0f, viewDistance - _FogOffset);
                fogFactor = exp2(-fogFactor * fogFactor);
                
                float4 fogOutput = lerp(_FogColor, col, saturate(fogFactor));
                
    return fogOutput;
}

            ENDHLSL
        }
    }
}
