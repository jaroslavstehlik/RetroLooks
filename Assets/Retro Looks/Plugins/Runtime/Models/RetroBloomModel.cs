using System;
using UnityEngine;

namespace RetroLooks
{
    [Serializable]
    public class RetroBloomModel : RetroLooksModel
    {
        [Serializable]
        public struct BloomSettings
        {
            [Min(0f), Tooltip("Blend factor of the result image.")]
            public float intensity;

            [Min(0f), Tooltip("Filters out pixels under this level of brightness.")]
            public float threshold;

            public float thresholdLinear
            {
                set { threshold = Mathf.LinearToGammaSpace(value); }
                get { return Mathf.GammaToLinearSpace(threshold); }
            }

            [Range(0f, 1f), Tooltip("Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).")]
            public float softKnee;

            [Range(1f, 7f), Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
            public float radius;
            
            public static BloomSettings defaultSettings
            {
                get
                {
                    return new BloomSettings
                    {
                        intensity = 0.5f,
                        threshold = 1.1f,
                        softKnee = 0.5f,
                        radius = 4f,
                    };
                }
            }
        }
        
        [Serializable]
        public struct Settings
        {
            public BloomSettings bloom;

            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        bloom = BloomSettings.defaultSettings,
                    };
                }
            }
        }

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public override void Reset()
        {
            m_Settings = Settings.defaultSettings;
        }
    }
}
