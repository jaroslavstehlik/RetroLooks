Shader "Retro Looks/Filters/Multiply"
{
	Properties
	{
		_LastTex("Texture", 2D) = "black" {}
		_MainTex ("Texture", 2D) = "white" {}		
		_Amount ("Amount", Range(0, 1)) = 0.5
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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _LastTex;
			float _Amount;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_LastTex, i.uv);
				return lerp(col, col * tex2D(_MainTex, i.uv), _Amount);
			}
			ENDCG
		}
	}
}
