Shader "Retro Looks/Filters/TVLines"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LineSize ("Size", Float) = 1
		_Amount ("Amount", Float) = 1
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
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _LineSize;
			float _Amount;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv;
				o.uv.zw = float2(v.uv.x, v.uv.y + _MainTex_TexelSize.y * _LineSize * 0.5);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float v = i.vertex.y / _ScreenParams.y;
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				fixed lineC = fmod(v / (_MainTex_TexelSize.y * 2), _LineSize) * (2 / (_LineSize * 1.25));
				col = lerp(col, col * lineC * lineC, _Amount);
				return col;
			}
			ENDCG
		}
	}
}
