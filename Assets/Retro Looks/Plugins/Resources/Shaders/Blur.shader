Shader "Retro Looks/Filters/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BaseTex("Texture", 2D) = "white" {}
		_Radius("Radius", Float) = 1
	}


	CGINCLUDE

	#pragma target 3.0
	#include "UnityCG.cginc"
	#include "Common.cginc"

	float _PrefilterOffs;
	float _Threshold;
	float3 _Curve;
	float _SampleScale;
	
	struct VaryingsMultitex
	{
		float4 pos : SV_POSITION;
		float2 uvMain : TEXCOORD0;
		float2 uvBase : TEXCOORD1;
	};

	VaryingsMultitex VertMultitex(AttributesDefault v)
	{
		VaryingsMultitex o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uvMain = UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);
		o.uvBase = o.uvMain;

#if UNITY_UV_STARTS_AT_TOP
		if (_BaseTex_TexelSize.y < 0.0)
			o.uvBase.y = 1.0 - o.uvBase.y;
#endif

		return o;
	}


	// Brightness function
	half Brightness(half3 c)
	{
		return Max3(c);
	}

	half4 FragPrefilter(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.uv + _MainTex_TexelSize.xy * _PrefilterOffs;
		half4 s0 = SafeHDR(tex2D(_MainTex, uv));
		half3 m = s0.rgb;

		#if UNITY_COLORSPACE_GAMMA
		m = GammaToLinearSpace(m);
		#endif

		// Pixel brightness
		half br = Brightness(m);

		// Under-threshold part: quadratic curve
		half rq = clamp(br - _Curve.x, 0.0, _Curve.y);
		rq = _Curve.z * rq * rq;

		// Combine and apply the brightness response curve.
		m *= max(rq, br - _Threshold) / max(br, 1e-5);

		return EncodeHDR(m);
	}


	half3 DownsampleFilter(sampler2D tex, float4 uv0, float4 uv1)
	{
		return	(DecodeHDR(tex2D(tex, uv0.xy))
				+ DecodeHDR(tex2D(tex, uv0.zw))
				+ DecodeHDR(tex2D(tex, uv1.xy))
				+ DecodeHDR(tex2D(tex, uv1.zw))) * 0.25;
	}
	
	/*
	half3 DownsampleFilter(sampler2D tex, float2 uv, float2 texelSize)
	{
		float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);

		half3 s;
		s = DecodeHDR(tex2D(tex, uv + d.xy));
		s += DecodeHDR(tex2D(tex, uv + d.zy));
		s += DecodeHDR(tex2D(tex, uv + d.xw));
		s += DecodeHDR(tex2D(tex, uv + d.zw));

		return s * (1.0 / 4.0);
	}
	*/
	half3 FragDownsample(VaryingsDefault i) : SV_Target
	{
		//return EncodeHDR(DownsampleFilter(_MainTex, i.uv, _MainTex_TexelSize.xy));
		return EncodeHDR(DownsampleFilter(_MainTex, i.uv0, i.uv1));
	}

	half2 random(half2 p) 
	{
		p = frac(p * half2(443.897, 441.423));
		p += dot(p, p.yx + 19.19);
		return frac((p.xx + p.yx)*p.xy);
	}

	//https://www.shadertoy.com/view/XtGGzz
	half3 FastDownsampleFilter(sampler2D tex, float2 uv, float texelSize)
	{
		half2 r = random(uv / texelSize);
		r.x *= 6.28305308;
		half2 cr = half2(sin(r.x), cos(r.x)) * sqrt(r.y);
		return DecodeHDR(tex2D(tex, uv + cr * _Radius / texelSize));
	}

	half3 FragFastDownsample(VaryingsDefault i) : SV_Target
	{
		return EncodeHDR(FastDownsampleFilter(_MainTex, i.uv.xy, i.uv.z));
	}

	half3 UpsampleFilter(sampler2D tex, float2 uv, float2 texelSize, float sampleScale)
	{
#if MOBILE_OR_CONSOLE
		// 4-tap bilinear upsampler
		float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0) * (sampleScale * 0.5);

		half3 s;
		s = DecodeHDR(tex2D(tex, uv + d.xy));
		s += DecodeHDR(tex2D(tex, uv + d.zy));
		s += DecodeHDR(tex2D(tex, uv + d.xw));
		s += DecodeHDR(tex2D(tex, uv + d.zw));

		return s * (1.0 / 4.0);
#else
		// 9-tap bilinear upsampler (tent filter)
		float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;

		half3 s;
		s = DecodeHDR(tex2D(tex, uv - d.xy));
		s += DecodeHDR(tex2D(tex, uv - d.wy)) * 2.0;
		s += DecodeHDR(tex2D(tex, uv - d.zy));

		s += DecodeHDR(tex2D(tex, uv + d.zw)) * 2.0;
		s += DecodeHDR(tex2D(tex, uv))        * 4.0;
		s += DecodeHDR(tex2D(tex, uv + d.xw)) * 2.0;

		s += DecodeHDR(tex2D(tex, uv + d.zy));
		s += DecodeHDR(tex2D(tex, uv + d.wy)) * 2.0;
		s += DecodeHDR(tex2D(tex, uv + d.xy));

		return s * (1.0 / 16.0);
#endif
	}

	half4 FragUpsample(VaryingsMultitex i) : SV_Target
	{
		half3 base = DecodeHDR(tex2D(_BaseTex, i.uvBase));
		half3 blur = UpsampleFilter(_MainTex, i.uvMain, _MainTex_TexelSize.xy, _SampleScale);
		return EncodeHDR(base + blur);
	}

	ENDCG

	SubShader
	{
		// No culling or depth
		ZTest Always Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex VertDefault
			#pragma fragment FragPrefilter
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex VertDefault
			#pragma fragment FragDownsample	
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex VertMultitex
			#pragma fragment FragUpsample	
			ENDCG
		}
	}
}
