using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVSimulator : MonoBehaviour
{
    public bool tvColor = true;
    public float tvColorAmount = 1f;
    public Vector2 tvColorOffsetR = new Vector2(1, 0);
    public Vector2 tvColorOffsetG = new Vector2(0, 1);
    public Vector2 tvColorOffsetB = new Vector2(-1, 0);
    [Range(0, 1)]
    public float tvColorGrainAmount = 1f;

    public bool tvGhost = true;
    public Vector2 ghostDisplacement = new Vector2(0.01f, 0f);
    [Range(0, 1)]
    public float ghostAmount = 0.2f;

    public bool tvLightStreaks = true;
    [Range(1, 8)]
    public int tvLightStreaksResolution = 1;
    [Range(1, 8)]
    public int tvLightStreaksIterations = 4;
    public Vector2 tvLightStreaksTreshold = new Vector2(0f, 1f);
    [Range(0, 1)]
    public float tvLightStreaksAmount = 1f;

    public bool tvLines = true;
    [Range(1, 10)]
    public float lineSize = 1f;
    [Range(0, 1)]
    public float linesAmount = 1f;

    public bool tvDistort = true;
    [Range(0, 1)]
    public float distortAmount = 1f;
    [Range(0, 10)]
    public float distortSize = 1f;
    [Range(0, 1)]
    public float zoom = 1f;
    
    public bool tvBleed = true;
    public float bleedRadius = 1f;
    public float bleedAmount = 1f;

    Material tvColorMaterial;
    Material tvGhostMaterial;
    Material tvLightStreakMaterial;
    Material tvLinesMaterial;
    Material tvDistortMaterial;
    Material kawaseMaterial;
    Material boxBlurMaterial;
    Material bleedCombineMaterial;

    RenderTexture rt0;
    RenderTexture rt1;
    RenderTexture rtLightStreak0;
    RenderTexture rtLightStreak1;

    private void OnEnable()
    {
        if (rt0 == null) rt0 = RT.Get(Screen.width, Screen.height);
        if (rt1 == null) rt1 = RT.Get(Screen.width, Screen.height);

        int tvRes = tvLightStreaksResolution * 8;
        if (rtLightStreak0 == null) rtLightStreak0 = RT.Get(tvRes, Screen.height);
        if (rtLightStreak1 == null) rtLightStreak1 = RT.Get(tvRes, Screen.height);

        if (tvGhostMaterial == null) tvGhostMaterial = new Material(Shader.Find(Shaders.TV_GHOST));
        if (tvLightStreakMaterial == null) tvLightStreakMaterial = new Material(Shader.Find(Shaders.TV_LIGHTSTREAK));
        if (tvColorMaterial == null) tvColorMaterial = new Material(Shader.Find(Shaders.TV_COLOR));
        if (tvLinesMaterial == null) tvLinesMaterial = new Material(Shader.Find(Shaders.TV_LINES));
        if (tvDistortMaterial == null) tvDistortMaterial = new Material(Shader.Find(Shaders.TV_DISTORT));
        if (kawaseMaterial == null) kawaseMaterial = new Material(Shader.Find(Shaders.KAWASE));
        if (boxBlurMaterial == null) boxBlurMaterial = new Material(Shader.Find(Shaders.BOX_BLUR));
        if (bleedCombineMaterial == null) bleedCombineMaterial = new Material(Shader.Find(Shaders.LERP));
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture output = source;
        RenderTexture temp = RT.GetFullTemp();
        Graphics.Blit(output, temp);

        if (tvColor)
        {
            tvColorMaterial.SetFloat("_Amount", tvColorAmount);
            tvColorMaterial.SetFloat("_GrainAmount", tvColorGrainAmount);            
            tvColorMaterial.SetTexture("_Noise", Noise.blueNoise);
            tvColorMaterial.SetVector("_OffsetR", tvColorOffsetR);
            tvColorMaterial.SetVector("_OffsetG", tvColorOffsetG);
            tvColorMaterial.SetVector("_OffsetB", tvColorOffsetB);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvColorMaterial);
            temp = output;
        }

        if (tvGhost)
        {
            tvGhostMaterial.SetVector("_Displacement", ghostDisplacement);
            tvGhostMaterial.SetFloat("_Amount", ghostAmount * 0.5f);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvGhostMaterial);
            temp = output;
        }

        if(tvLightStreaks)
        {            
            Graphics.Blit(temp, rtLightStreak0);
            tvLightStreakMaterial.SetTexture("_LightStreakTex", rtLightStreak0);

            for (int i = 0; i < tvLightStreaksIterations; i++)
            {
                Graphics.Blit(rtLightStreak0, rtLightStreak1, boxBlurMaterial);
                Graphics.Blit(rtLightStreak1, rtLightStreak0, boxBlurMaterial);
            }

            tvLightStreakMaterial.SetVector("_Treshold", tvLightStreaksTreshold);
            tvLightStreakMaterial.SetFloat("_Amount", tvLightStreaksAmount);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvLightStreakMaterial);
            temp = output;
        }

        if (tvLines)
        {
            tvLinesMaterial.SetFloat("_LineSize", lineSize);
            tvLinesMaterial.SetFloat("_Amount", linesAmount);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvLinesMaterial);
            temp = output;
        }

        if (tvDistort)
        {
            tvDistortMaterial.SetFloat("_Amount", distortAmount);
            tvDistortMaterial.SetFloat("_Size", distortSize);
            tvDistortMaterial.SetFloat("_Zoom", 1 - zoom);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvDistortMaterial);
            temp = output;
        }

        if (tvBleed)
        {
            Graphics.Blit(temp, source);
            boxBlurMaterial.SetVector("_Direction", new Vector2(0f, bleedRadius));
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, boxBlurMaterial);
            temp = output;
            output = RT.GetFullTemp();
            bleedCombineMaterial.SetTexture("_LastTex", source);
            bleedCombineMaterial.SetFloat("_Amount", bleedAmount);
            Graphics.Blit(temp, output, bleedCombineMaterial);
            temp = output;
        }

        Graphics.Blit(output, destination);
    }
}
