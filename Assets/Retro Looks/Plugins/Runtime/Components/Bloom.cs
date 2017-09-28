using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Bloom : MonoBehaviour
{
    public bool hdr = true;
    [Range(1, 8)]
    public int downsampleX = 4;
    [Range(1, 8)]
    public int downsampleY = 4;
    [Range(1, 8)]
    public int iterations = 1;
    [Range(0, 1)]
    public float temporalFlicker = 0.9f;
    [Range(0, 1)]
    public float noiseAmount = 1f;
    [Range(0, 20)]
    public float power = 2f;
    public Vector2 treshold = new Vector2(0.9f, 1f);
    [Range(0, 10)]
    public float bloom = 1f;
    [Range(0, 1)]
    public float saturation = 1f;
    [Range(0, 1)]
    public float amount = 1f;   

    Material scaleMaterial;
    Material kawaseMaterial;
    Material bloomMaterial;
    Material bloomPreprocessMaterial;
    Material lerpMaterial;

    RenderTexture blit0;
    RenderTexture blit1;
    RenderTexture blit0Half;
    RenderTexture blit1Half;
    RenderTexture lastFrame;
    Camera camera;
    
    private void OnEnable()
    {        
        if (scaleMaterial == null) scaleMaterial = new Material(Shader.Find(Shaders.SCALE));
        if (kawaseMaterial == null) kawaseMaterial = new Material(Shader.Find(Shaders.KAWASE));
        if (bloomPreprocessMaterial == null) bloomPreprocessMaterial = new Material(Shader.Find(Shaders.BLOOM_PREPROCESS));
        if (bloomMaterial == null) bloomMaterial = new Material(Shader.Find(Shaders.BLOOM));
        if (lerpMaterial == null) lerpMaterial = new Material(Shader.Find(Shaders.LERP));

        InitRenderTextures();
    }

    private void OnValidate()
    {
        InitRenderTextures();
    }

    public void InitRenderTextures()
    {
        if (camera == null) camera = GetComponent<Camera>();

        int width = camera.pixelWidth / downsampleX, height = camera.pixelHeight / downsampleY;
        RenderTextureFormat format = hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

        blit0 = RT.Get(width / 2, height / 2, false, format);
        blit1 = RT.Get(width / 2, height / 2, false, format);
        blit0Half = RT.Get(width / 8, height / 8, false, format);
        blit1Half = RT.Get(width / 8, height / 8, false, format);
        lastFrame = RT.Get(width / 2, height / 2, false, format);
    }

    public void Kawase(RenderTexture rtIn, RenderTexture rtOut, int iterations = 4)
    {
        for (int i = 0; i < iterations; i++)
        {
            kawaseMaterial.SetFloat("_Radius", 0);
            Graphics.Blit(rtIn, rtOut, kawaseMaterial);
            kawaseMaterial.SetFloat("_Radius", 1);
            Graphics.Blit(rtOut, rtIn, kawaseMaterial);
            kawaseMaterial.SetFloat("_Radius", 2);
            Graphics.Blit(rtIn, rtOut, kawaseMaterial);
            kawaseMaterial.SetFloat("_Radius", 2);
            Graphics.Blit(rtOut, rtIn, kawaseMaterial);
            kawaseMaterial.SetFloat("_Radius", 3);
            Graphics.Blit(rtIn, rtOut, kawaseMaterial);
            Graphics.Blit(rtOut, rtIn);
        }

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, blit0);
        Kawase(blit0, blit1, iterations);

        Graphics.Blit(blit0, blit0Half);
        Kawase(blit0Half, blit1Half, iterations);

        bloomPreprocessMaterial.SetTexture("_BlurWide", blit0Half);
        bloomPreprocessMaterial.SetTexture("_BlurThin", blit0);
        bloomPreprocessMaterial.SetFloat("_Power", power);

        Graphics.Blit(blit0, blit1, bloomPreprocessMaterial);
        Graphics.Blit(blit1, blit0);

        if (temporalFlicker > 0)
        {
            lerpMaterial.SetTexture("_LastTex", lastFrame);
            lerpMaterial.SetFloat("_Amount", 1f - temporalFlicker);
            Graphics.Blit(blit0, blit1, lerpMaterial);
            Graphics.Blit(blit1, lastFrame);                             
        }

        bloomMaterial.SetTexture("_BlurTex", blit1);
        bloomMaterial.SetTexture("_Noise", Noise.blueNoise);
        bloomMaterial.SetFloat("_NoiseAmount", noiseAmount);
        bloomMaterial.SetVector("_Treshold", treshold);
        bloomMaterial.SetFloat("_Bloom", bloom * 100);
        bloomMaterial.SetFloat("_Saturation", saturation);
        bloomMaterial.SetFloat("_Amount", amount);
        Graphics.Blit(source, destination, bloomMaterial);
    }
}
