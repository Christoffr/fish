Shader "Tutorial/BasicTexturing"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _BaseTexture("Base Texture", 2D) = "white" {}
        _Amplitude ("Amplitude", Float) = 0.1
        _Frequency ("Frequency", Float) = 1.0
        _Speed ("Speed", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

                HLSLPROGRAM
                   #pragma vertex vert
                   #pragma fragment frag

                   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                   struct appdata
                   {
                       float4 positionOS    : POSITION;
                       float3 normal        : NORMAL;
                       float2 uv            : TEXCOORD0;
                   };
               
                   struct v2f
                   {
                      float4 positionCS     : SV_POSITION;
                      float2 uv             : TEXCOORD0;
                   };

                   TEXTURE2D(_BaseTexture);
                   SAMPLER(sampler_BaseTexture);
               
                   CBUFFER_START(UnityPerMaterial)
                        half4 _BaseColor;
                        float4 _BaseTexture_ST;
                        float _Amplitude;
                        float _Frequency;
                        float _Speed;
                   CBUFFER_END
               
                   v2f vert (appdata v)
                   {
                      v2f o;

                      // Side to side movement
                      float offset = sin(_Time.y * _Speed) * _Amplitude;
                      v.positionOS.x += offset;

                      o.positionCS = TransformObjectToHClip(v.positionOS);
                      o.uv = TRANSFORM_TEX(v.uv, _BaseTexture);

                      return o;
                   }

                   half4 frag(v2f i) : SV_Target
                   {
                        half4 tex = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, i.uv);
                        return _BaseColor * tex;
                   }
                ENDHLSL
        }
    }
}