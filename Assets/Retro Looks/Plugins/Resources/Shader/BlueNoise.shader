Shader "Retro Looks/Filters/BlueNoise"
{
	Properties
	{
		_NoiseA ("Noise A", 2D) = "white" {}
		_NoiseB ("Noise B", 2D) = "white" {}
		_Speed ("Speed", Float) = 10
	}
	
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{				
				float4 vertex : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
			};

			sampler2D _NoiseA;
			sampler2D _NoiseB;
			float _Amount;
			float _Speed;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0.x = max(_ScreenParams.x, _ScreenParams.y) / 256;
				fixed4 scales = (0.004, 1.134, 1.4285, 1);
				o.uv1 = fmod(_Time * _Speed * scales, 8);
				return o;
			}

			fixed Lerp8(float time, fixed4 texA, fixed4 texB)
			{
				fixed output = 0;
				output += dot(texA, saturate(1 - abs(time - float4(0, 1, 2, 3))));
				output += dot(texB, saturate(1 - abs(time - float4(4, 5, 6, 7))));
				output += texA.r * saturate(1 - abs(time - 8));
				return output;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.vertex.xy / i.uv0.x;
				fixed4 texA = tex2D(_NoiseA, uv);
				fixed4 texB = tex2D(_NoiseB, uv);
					
				return fixed4(Lerp8(i.uv1.x, texA, texB), Lerp8(i.uv1.y, texA, texB), Lerp8(i.uv1.z, texA, texB), Lerp8(i.uv1.w, texA, texB));
			}

			ENDCG
		}
	}
}
