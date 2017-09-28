Shader "Retro Looks/Filters/TVDistort"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Float) = 1
		_Zoom("Size", Float) = 1
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
			float _Size;
			float _Zoom;


			float2 Distortion(float2 uv)
			{
				float2 uv2 = (uv - 0.5f);
				float dist = dot(uv2, uv2) * _Size;
				uv = uv + uv2 * (1.0f + dist) * dist;
				uv -= 0.5;
				uv *= _Zoom;
				uv += 0.5;
				return uv;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv;
				o.uv.zw = (+0.5 - v.uv) * 2;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, lerp(i.uv, Distortion(i.uv), _Amount));
				return col;
			}
			ENDCG
		}
	}
}
