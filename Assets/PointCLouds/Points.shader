Shader "Unlit/Points"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			struct ParticleData {
				float4 Position;
				float4 Color;
			};

			StructuredBuffer<ParticleData> _ParticleDataBuff;
			float4x4 My_Object2World;
			float4 _FogColorc;
			float _ParticleSize;
			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

            v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
            {
				v2f o;
				ParticleData p = _ParticleDataBuff[inst];

				float3 pos = p.Position.xyz;
				float size = p.Position.w;
				size = (1.0-smoothstep(1000., 0.0 , size))*5.5 +2. ;
				size *= _ParticleSize;
				float3 upDir = float3(0, 1.,0.)*size;
				float3 rightDir = float3(0,0,1.)*size;
				float3 forwDir = float3(1, 0, 0)*size;
			

				[branch] switch (id) {
				case 0: pos = pos - forwDir; break;
				case 1: pos = pos + upDir; break;
				case 2: pos = pos + rightDir; break;

				case 3: pos = pos - forwDir; break;
				case 4: pos = pos + upDir; break;
				case 5: pos = pos - rightDir; break;

				case 6: pos = pos - forwDir; break;
				case 7: pos = pos + rightDir; break;
				case 8: pos = pos - rightDir; break;

				case  9: pos = pos + upDir; break;
				case 10: pos = pos + rightDir; break;
				case 11: pos = pos - rightDir; break;
				};

             
                o.vertex = mul(UNITY_MATRIX_VP, mul(My_Object2World, float4(pos.xyz,1.)));
				o.color = lerp( p.Color, _FogColorc, smoothstep(5.,2500.,p.Position.w));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color;
     
                return col;
            }
            ENDCG
        }
    }
}
