Shader "Retro Looks/Filters/BoxBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Direction ("Direction", Vector) = (1, 0, 0, 0)
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
				float4 vertex : SV_POSITION;
			};


			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float2 _Direction;

			v2f vert (appdata v)
			{
				v2f o;
				float texel = _MainTex_TexelSize * 0.5;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0.xy = float2(v.uv.x, v.uv.y) - texel * _Direction;
				o.uv0.zw = float2(v.uv.x, v.uv.y) + texel * _Direction;				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return (tex2D(_MainTex, i.uv0.xy) + tex2D(_MainTex, i.uv0.zw)) * 0.5;
			}
			ENDCG
		}
	}
}
