using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VintageCamera : MonoBehaviour
{
    public bool tvLines = true;
    [Range(1, 10)]
    public float lineSize = 1f;
    [Range(0, 1)]
    public float amount = 1f;

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

    Material tvlinesMaterial;
    Material tvDistortMaterial;
    Material kawaseMaterial;
    Material bleedCombineMaterial;

    RenderTexture rt0;
    RenderTexture rt1;

    private void OnEnable()
    {
        if (rt0 == null) rt0 = RT.Get(Screen.width, Screen.height);
        if (rt1 == null) rt1 = RT.Get(Screen.width, Screen.height);
        if (tvlinesMaterial == null) tvlinesMaterial = new Material(Shader.Find(Shaders.TV_LINES));
        if (tvDistortMaterial == null) tvDistortMaterial = new Material(Shader.Find(Shaders.TV_DISTORT));
        if (kawaseMaterial == null) kawaseMaterial = new Material(Shader.Find(Shaders.KAWASE));
        if (bleedCombineMaterial == null) bleedCombineMaterial = new Material(Shader.Find(Shaders.LERP));
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture output = source;
        RenderTexture temp = RT.GetFullTemp();
        Graphics.Blit(output, temp);
        if (tvLines)
        {
            tvlinesMaterial.SetFloat("_LineSize", lineSize);
            tvlinesMaterial.SetFloat("_Amount", amount);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, tvlinesMaterial);
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
            kawaseMaterial.SetFloat("_Radius", bleedRadius);
            output = RT.GetFullTemp();
            Graphics.Blit(temp, output, kawaseMaterial);
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
