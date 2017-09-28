Shader "Retro Looks/Filters/Kawase"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Radius ("Radius", Float) = 1
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
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};


			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _Radius;

			v2f vert (appdata v)
			{
				v2f o;
				float texel = _MainTex_TexelSize * 0.5 * _Radius;
				o.vertex = UnityObjectToClipPos(v.vertex);				
				o.uv0.xy = float2(v.uv.x, v.uv.y) - texel;
				o.uv0.zw = float2(v.uv.x, v.uv.y) + texel;
				o.uv1.xy = float2(v.uv.x + texel, v.uv.y - texel);
				o.uv1.zw = float2(v.uv.x - texel, v.uv.y + texel);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return (tex2D(_MainTex, i.uv0.xy) + tex2D(_MainTex, i.uv0.zw) + tex2D(_MainTex, i.uv1.xy) + tex2D(_MainTex, i.uv1.zw)) * 0.25;
			}
			ENDCG
		}
	}
}
