using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    protected static Material _blueNoiseMaterial;
    protected static Material blueNoiseMaterial
    {
        get
        {
            if (_blueNoiseMaterial == null) _blueNoiseMaterial = Resources.Load<Material>("blueNoise");
            return _blueNoiseMaterial;
        }
    }

    protected static int _lastBlueNoiseFrame;
    protected static RenderTexture _blueNoise;
    public static RenderTexture blueNoise
    {
        get
        {
            if(_blueNoise == null) _blueNoise =  RT.Get(Screen.width, Screen.height);
            if (_lastBlueNoiseFrame != Time.frameCount)
            {
                Graphics.Blit(null, _blueNoise, blueNoiseMaterial);
                _lastBlueNoiseFrame = Time.frameCount;
            }
            return _blueNoise;
        }
    }
}
