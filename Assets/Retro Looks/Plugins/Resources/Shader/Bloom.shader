Shader "Retro Looks/Filters/Bloom"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_BlurTex("Blue Texture", 2D) = "black" {}
		_Noise ("Noise Texture", 2D) = "black" {}
		_NoiseAmount("Noise Amount", Float) = 1
		_Treshold ("Treshold", Vector) = (0.9, 1, 0, 0)
		_Bloom ("Bloom", Float) = 100		
		_Saturation ("Saturation", Float) = 1
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
			sampler2D _BlurTex;
			sampler2D _Noise;
			float _NoiseAmount;
			float2 _Treshold;
			float _Bloom;	
			float _Saturation;
			float _Amount;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed noise = tex2D(_Noise, i.uv).a;
				fixed4 orig = tex2D(_MainTex, i.uv);
				fixed3 col = orig;
				fixed3 blur = tex2D(_BlurTex, i.uv);
				fixed intensity = dot(blur, float3(0.3, 0.59, 0.11));

				blur = smoothstep(_Treshold.x, _Treshold.y, intensity) * blur;
				col = lerp(orig, orig * (1 + blur * _Bloom), _Saturation);
				col += blur * _Bloom * 0.01;
				col = lerp(col, col * noise, _NoiseAmount);
				col = lerp(orig.rgb, col, _Amount);
				return fixed4(col, orig.a);
			}
			ENDCG
		}
	}
}
