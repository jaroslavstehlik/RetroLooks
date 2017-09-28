using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroLooks
{
    public class RetroBloomComponent : RetroLooksComponentRenderTexture<RetroBloomModel>
    {
        static class Uniforms
        {
            internal static readonly int _Threshold = Shader.PropertyToID("_Threshold");
            internal static readonly int _Curve = Shader.PropertyToID("_Curve");
            internal static readonly int _SampleScale = Shader.PropertyToID("_SampleScale");
            internal static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");
            internal static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");
            internal static readonly int _Bloom_Settings = Shader.PropertyToID("_Bloom_Settings");            
        }

        const int k_MaxPyramidBlurLevel = 16;
        readonly RenderTexture[] m_BlurBuffer1 = new RenderTexture[k_MaxPyramidBlurLevel];
        readonly RenderTexture[] m_BlurBuffer2 = new RenderTexture[k_MaxPyramidBlurLevel];

        public override bool active
        {
            get
            {
                return model.enabled
                       && model.settings.bloom.intensity > 0f
                       && !context.interrupted;
            }
        }

        public void Prepare(RenderTexture source, Material retroMaterial)
        {
            var bloom = model.settings.bloom;
            var material = context.materialFactory.Get("Retro Looks/Filters/Blur");
            material.shaderKeywords = null;
            
            // Do bloom on a half-res buffer, full-res doesn't bring much and kills performances on
            // fillrate limited platforms
            var tw = context.width / 2;
            var th = context.height / 2;

            // Blur buffer format
            // TODO: Extend the use of RGBM to the whole chain for mobile platforms
            var useRGBM = Application.isMobilePlatform;
            var rtFormat = useRGBM
                ? RenderTextureFormat.Default
                : RenderTextureFormat.DefaultHDR;

            // Determine the iteration count
            float logh = Mathf.Log(th, 2f) + bloom.radius - 8f;
            int logh_i = (int)logh;
            int iterations = Mathf.Clamp(logh_i, 1, k_MaxPyramidBlurLevel);
            
            // update the shader properties
            float lthresh = bloom.thresholdLinear;
            material.SetFloat(Uniforms._Threshold, lthresh);
            
            float knee = lthresh * bloom.softKnee + 1e-5f;
            var curve = new Vector3(lthresh - knee, knee * 2f, 0.25f / knee);
            material.SetVector(Uniforms._Curve, curve);
            
            float sampleScale = 0.5f + logh - logh_i;
            material.SetFloat(Uniforms._SampleScale, sampleScale);
            
            // Prefilter pass
            var prefiltered = context.renderTextureFactory.Get(tw, th, 0, rtFormat);
            Graphics.Blit(source, prefiltered, material, 0);
            
            // Construct a mip pyramid
            var last = prefiltered;

            for (int level = 0; level < iterations; level++)
            {
                m_BlurBuffer1[level] = context.renderTextureFactory.Get(
                        last.width / 2, last.height / 2, 0, rtFormat
                        );

                Graphics.Blit(last, m_BlurBuffer1[level], material, 1);

                last = m_BlurBuffer1[level];
            }

            // Upsample and combine loop
            for (int level = iterations - 2; level >= 0; level--)
            {
                var baseTex = m_BlurBuffer1[level];
                material.SetTexture(Uniforms._BaseTex, baseTex);

                m_BlurBuffer2[level] = context.renderTextureFactory.Get(
                        baseTex.width, baseTex.height, 0, rtFormat
                        );

                Graphics.Blit(last, m_BlurBuffer2[level], material, 2);
                last = m_BlurBuffer2[level];
            }

            var bloomTex = last;

            // Release the temporary buffers
            for (int i = 0; i < k_MaxPyramidBlurLevel; i++)
            {
                if (m_BlurBuffer1[i] != null)
                    context.renderTextureFactory.Release(m_BlurBuffer1[i]);

                if (m_BlurBuffer2[i] != null && m_BlurBuffer2[i] != bloomTex)
                    context.renderTextureFactory.Release(m_BlurBuffer2[i]);

                m_BlurBuffer1[i] = null;
                m_BlurBuffer2[i] = null;
            }

            //context.renderTextureFactory.Release(prefiltered);

            // Push everything to the uber material
            retroMaterial.SetTexture(Uniforms._BloomTex, bloomTex);
            retroMaterial.SetVector(Uniforms._Bloom_Settings, new Vector2(sampleScale, bloom.intensity));

            retroMaterial.EnableKeyword("BLOOM");
        }
    }
}
