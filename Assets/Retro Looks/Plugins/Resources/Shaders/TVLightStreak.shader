Shader "Retro Looks/Filters/TVLightStreak"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LightStreakTex("Light Streak", 2D) = "white" {}
		_Treshold ("Treshold", Vector) = (0, 1, 0, 0)
		_Amount("Amount", Float) = 1
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv0 : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _LightStreakTex;
			float2 _Treshold;
			float _Amount;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv0);
				fixed intensity = dot(col.rgb, float3(0.3, 0.59, 0.11));				
				return col + smoothstep(_Treshold.x, _Treshold.y, intensity) * tex2D(_LightStreakTex, i.uv0) * _Amount;;
			}
		ENDCG
		}
	}
}
