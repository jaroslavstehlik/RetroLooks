Shader "Retro Looks/Filters/TVColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Noise ("Noise", 2D) = "white" {}
		_OffsetR ("OffsetR", Vector) = (1, 0, 0, 0)
		_OffsetG ("OffsetG", Vector) = (0, 1, 0, 0)
		_OffsetB ("OffsetB", Vector) = (-1, 0, 0, 0)
		_GrainAmount("Grain Amount", Float) = 1
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
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _Noise;
			float _Amount;			

			float2 _OffsetR;
			float2 _OffsetG;
			float2 _OffsetB;
			float _GrainAmount;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0.xy = v.uv;
				o.uv0.zw = v.uv + _OffsetR * _MainTex_TexelSize.xy;
				o.uv1.xy = v.uv + _OffsetG * _MainTex_TexelSize.xy;
				o.uv1.zw = v.uv + _OffsetB * _MainTex_TexelSize.xy;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 0;
				col.r = tex2D(_MainTex, i.uv0.zw).r;
				col.g = tex2D(_MainTex, i.uv1.xy).g;
				col.b = tex2D(_MainTex, i.uv1.zw).b;
				
				fixed3 yuv;
				yuv.x = dot(col.rgb, half3(0.299, 0.587, 0.114));
				yuv.y = (col.b - yuv.x) * 0.492;
				yuv.z = (col.r - yuv.x) * 0.877;

				fixed4 grain = tex2D(_Noise, i.uv0.xy);
				fixed2 grainAmt = lerp(1, (1 - _Amount.x) + grain.ba * _Amount.x * 2, _GrainAmount);
				yuv.r *= grainAmt.r;
				yuv.b *= grainAmt.g;
				
				col.r = yuv.z * 1.140 + yuv.x;
				col.g = yuv.z * (-0.581) + yuv.y * (-0.395) + yuv.x;
				col.b = yuv.y * 2.032 + yuv.x;

				return col;
			}
			ENDCG
		}
	}
}
