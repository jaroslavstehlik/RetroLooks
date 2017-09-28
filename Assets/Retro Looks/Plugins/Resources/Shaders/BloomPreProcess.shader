Shader "Retro Looks/Filters/BloomPreprocess"
{
	Properties
	{
		_BlurWide ("Blur Wide", 2D) = "black" {}
		_BlurThin ("Blur Thin", 2D) = "black" {}
		_Power ("Power", Float) = 2
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
			
			sampler2D _BlurThin;
			sampler2D _BlurWide;
			float _Power;

			fixed4 frag (v2f i) : SV_Target
			{			
				fixed4 blurs = pow(((tex2D(_BlurWide, i.uv) * 2 + tex2D(_BlurThin, i.uv)) * _Power), _Power);
				return blurs;
			}
			ENDCG
		}
	}
}
