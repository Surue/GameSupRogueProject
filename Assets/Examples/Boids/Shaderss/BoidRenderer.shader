Shader "Custom/BoidRendererURP"
{
    Properties
    {
        _SlowColor ("Slow Color", Color) = (0,0,1,1)
        _FastColor ("Fast Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Boid {
                float2 position;
                float2 velocity;
                float2 direction;
            };

            StructuredBuffer<Boid> _BoidBuffer;
            float4 _SlowColor;
            float4 _FastColor;
            float _MaxSpeedRef;

            struct Attributes
            {
                float4 positionOS : POSITION;
                uint instanceID : SV_InstanceID; 
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR0;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                uint id = input.instanceID; 
                Boid b = _BoidBuffer[id];

                float currentSpeed = length(b.velocity);
                
                float speedStep = saturate(currentSpeed / _MaxSpeedRef);
                
                output.color = lerp(_SlowColor, _FastColor, speedStep);
                
                float3 boidPos = float3(b.position.x, b.position.y, 0);
                float2 dir = normalize(b.velocity + float2(0.00001, 0)); 
                float3 forward = float3(dir.x, dir.y, 0);
                float3 up = float3(0, 0, -1);
                float3 right = cross(up, forward);

                float3 rotatedPos = input.positionOS.x * right + 
                                    input.positionOS.y * forward + 
                                    input.positionOS.z * up;

                float3 worldPos = rotatedPos + boidPos;
                output.positionCS = TransformWorldToHClip(worldPos);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}