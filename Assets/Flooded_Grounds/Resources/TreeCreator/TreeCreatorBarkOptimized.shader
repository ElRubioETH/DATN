Shader "URP/Nature/TreeBarkURP"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap ("Normalmap (GA) Spec (R)", 2D) = "bump" {}
        _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _BumpSpecMap;
            float4 _Color;
            float4 _TreeInstanceColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 c = tex2D(_MainTex, IN.uv);
                float3 baseColor = c.rgb * _TreeInstanceColor.rgb * _TreeInstanceColor.a;

                half4 normalTex = tex2D(_BumpSpecMap, IN.uv);
                float3 normalTS = UnpackNormal(normalTex);
                float3 normalWS = normalize(normalTS); // Fake normal mapping

                float3 lightDir = normalize(_MainLightPosition.xyz);
                float NdotL = saturate(dot(normalWS, lightDir));
                float3 lighting = baseColor * _MainLightColor.rgb * NdotL;

                return half4(lighting, c.a);
            }
            ENDHLSL
        }
    }

    FallBack "Diffuse"
}
