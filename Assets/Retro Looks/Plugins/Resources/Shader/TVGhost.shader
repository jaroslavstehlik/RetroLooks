Shader "Retro Looks/Filters/TVGhost"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Displacement("Displacement", Vector) = (1, 0, 0, 0)
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
				float4 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float2 _Displacement;
			float _Amount;

			v2f vert(appdata v)
			{
				v2f o;
				float texel = _MainTex_TexelSize;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0.xy = v.uv;
				o.uv1.xy = v.uv - texel * _Displacement;
				o.uv1.zw = v.uv + texel * _Displacement;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv0);
				return col + col * (tex2D(_MainTex, i.uv1.xy) * -2 + tex2D(_MainTex, i.uv1.zw) * 2) * _Amount;
			}
		ENDCG
		}
	}
}
