Shader "Tutorial/BasicTexturing"
{
    Properties
    {
        [Header(Color)]
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _BaseTexture("Base Texture", 2D) = "white" {}

        [Header(Movement)]
        _YawAmplitude ("Yaw Amplitude", Float) = 0.1
        _PanningYawApmlitude("Panning Yaw Amplitude", Float) = 0.1
        _RollAmplitude ("Roll Amplitude", Float) = 0.05
        _WaveFreq ("Wave Frequency", Float) = 0.1
        _SideAmplitude ("Side-to-Side Amplitude", Float) = 1.0
        _Speed ("Speed", Float) = 1.0

        [Header(Masking)]
        _MaskCenter ("Mask Center", Float) = 0.0
        _MaskFalloff ("Mask Falloff", Float) = 1.0

        [Header(Visualization)]
        [Toggle] _ShowMask ("Show Mask", Float) = 0
        _MaskColor ("Mask Color", Color) = (1,0,0,1)
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
            #pragma shader_feature _SHOWMASK_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS       : POSITION;
                float3 normal           : NORMAL;
                float2 uv               : TEXCOORD0;
            };
        
            struct v2f
            {
               float4 positionCS        : SV_POSITION;
               float2 uv                : TEXCOORD0;
               float maskValue          : TEXCOORD1;
            };

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);
        
            CBUFFER_START(UnityPerMaterial)
                 half4 _BaseColor;
                 float4 _BaseTexture_ST;
                 float _YawAmplitude;
                 float _PanningYawApmlitude;
                 float _RollAmplitude;
                 float _WaveFreq;
                 float _SideAmplitude;
                 float _Speed;
                 float _MaskCenter;
                 float _MaskFalloff;
                 half4 _MaskColor;
                 float3 _Bounds;
            CBUFFER_END

            // Yaw rotation matrix
            float3 rotateAroundYAxis(float3 vertex, float amplitude, float speed)
            {
                float angle = sin(_Time.y * speed) * amplitude;
                float cosA = cos(angle);
                float sinA = sin(angle);
                
                // Y-axis rotation matrix
                float3x3 rotationMatrix = float3x3(
                     cosA, 0, sinA,
                     0,    1, 0,
                    -sinA, 0, cosA
                );
                
                return mul(rotationMatrix, vertex);
            }

            // Panning Yaw rotation matrix
            float3 panningRotateAroundYAxis(float3 vertex, float amplitude, float speed, float waveFreq)
            {
                float angle = sin((_Time.y * speed) + (vertex.y * waveFreq)) * amplitude;
                float cosA = cos(angle);
                float sinA = sin(angle);
                
                // Y-axis rotation matrix
                float3x3 rotationMatrix = float3x3(
                     cosA, 0, sinA,
                     0,    1, 0,
                    -sinA, 0, cosA
                );
                
                return mul(rotationMatrix, vertex);
            }

            //Panning Roll rotation matrix
            float3 rotateAroundZAxis(float3 vertex, float amplitude, float speed, float waveFreq)
            {
                // Scale down the vertex.z contribution to prevent extreme rotations
                float angle = sin((_Time.y * speed) + (vertex.z * waveFreq)) * amplitude;
                float cosA = cos(angle);
                float sinA = sin(angle);
                
                // Z-axis rotation matrix
                float3x3 rotationMatrix = float3x3(
                    cosA, -sinA, 0,
                    sinA,  cosA, 0,
                    0,     0,    1
                );
                
                return mul(rotationMatrix, vertex);
            }

            // Side to side movement
            float3 sideToSideOffset(float3 pos, float amplitude, float speed)
            {
                float offset = sin(_Time.y * speed) * amplitude;
                return pos + float3(offset, 0, 0);
            }
            
        
            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
               v2f o;

               // Animate the mesh in object space
               float3 animated = v.positionOS.xyz;

               // Create smooth gradient mask
               float maskValue = saturate((animated.z - _MaskCenter) / _MaskFalloff);
               o.maskValue = maskValue;

               // Apply transformations in order
               animated = rotateAroundYAxis(animated, _YawAmplitude, _Speed);
               animated = rotateAroundZAxis(animated, _RollAmplitude * maskValue, _Speed, _WaveFreq);
               animated = panningRotateAroundYAxis(animated, _PanningYawApmlitude * maskValue, _Speed, _WaveFreq);
               animated = sideToSideOffset(animated, _SideAmplitude, _Speed);

               // Offset in grid based on instanceID
               int ix = instanceID % _Bounds.x;                    // X index
               int iy = (instanceID / _Bounds.x) % _Bounds.y;          // Y index
               int iz = instanceID / (_Bounds.x * _Bounds.y);          // Z index

               animated.xyz += float3(ix, iy, iz);

               // Transform to clip space
               o.positionCS = TransformObjectToHClip(animated);

               // UVs unchanged
               o.uv = TRANSFORM_TEX(v.uv, _BaseTexture);

               return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                 half4 tex = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, i.uv);
                 half4 baseColor = _BaseColor * tex;

                 #ifdef _SHOWMASK_ON
                    // Show mask visualization
                    // Black = no effect (mask = 0), Mask Color = full effect (mask = 1)
                    half4 maskVisualization = lerp(half4(0,0,0,1), _MaskColor, i.maskValue);
                    return lerp(baseColor, maskVisualization, 0.8); // Blend with base for visibility
                #else
                    // Normal rendering
                    return baseColor;
                #endif
            }
            ENDHLSL
        }
    }
}