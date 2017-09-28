using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RT
{
    protected static RenderTexture rt0;
    protected static RenderTexture rt1;

    public static RenderTexture GetSquared(int size)
    {
        RenderTexture rt = new RenderTexture(size, size, 0, RenderTextureFormat.Default);
        rt.antiAliasing = 1;
        rt.autoGenerateMips = false;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        return rt;
    }

    public static RenderTexture Get(int width, int height, bool mipmaps = false, RenderTextureFormat format = RenderTextureFormat.Default)
    {
        RenderTexture rt = new RenderTexture(width, height, 0, format);
        rt.antiAliasing = 1;
        rt.useMipMap = mipmaps;
        rt.autoGenerateMips = mipmaps;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        return rt;
    }

    protected static RenderTexture lastFullTemp;
    public static RenderTexture GetFullTemp()
    {
        if (rt0 == null) rt0 = Get(Screen.width, Screen.height);
        if (rt1 == null) rt1 = Get(Screen.width, Screen.height);
        if (lastFullTemp == null)
        {
            lastFullTemp = rt0;
            return lastFullTemp;
        }
        if (lastFullTemp == rt0)
        {
            lastFullTemp = rt1;
        } else
        {
            lastFullTemp = rt0;
        }
        return lastFullTemp;
    }
}
