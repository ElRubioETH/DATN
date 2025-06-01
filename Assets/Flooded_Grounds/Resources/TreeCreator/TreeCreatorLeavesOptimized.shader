Shader "URP/Nature/TreeLeavesURP"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.3
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap ("Normalmap (GA)", 2D) = "bump" {}
        _TranslucencyMap ("Trans (B) Gloss(A)", 2D) = "white" {}
        _TreeInstanceColor ("Tree Instance Color", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
        LOD 200
        Cull Off
        AlphaToMask On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
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
                float3 viewDirWS : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _BumpSpecMap;
            sampler2D _TranslucencyMap;
            float4 _Color;
            float4 _TreeInstanceColor;
            float4 _TranslucencyColor;
            float _Cutoff;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.uv);
                clip(col.a - _Cutoff);

                float3 normalTS = UnpackNormal(tex2D(_BumpSpecMap, IN.uv));
                float3 normalWS = normalize(normalTS); // approximation

                float NdotL = saturate(dot(normalWS, _MainLightPosition.xyz));
                float3 baseColor = col.rgb * _Color.rgb * _TreeInstanceColor.rgb * _TreeInstanceColor.a;

                // View-dependent translucency
                float viewFactor = pow(saturate(dot(normalWS, IN.viewDirWS)), 12);
                float3 transColor = _TranslucencyColor.rgb * (1.0 - viewFactor);

                float3 finalColor = baseColor * (_MainLightColor.rgb * NdotL) + transColor * 0.02;

                return half4(finalColor, col.a);
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Cutoff;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half alpha = tex2D(_MainTex, IN.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Diffuse"
}
