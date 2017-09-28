using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroLooks
{
    public class RetroLooksContext
    {
        public RetroLooksProfile profile;
        public Camera camera;

        public MaterialFactory materialFactory;
        public RenderTextureFactory renderTextureFactory;

        public bool interrupted { get; private set; }

        public void Interrupt()
        {
            interrupted = true;
        }

        public RetroLooksContext Reset()
        {
            profile = null;
            camera = null;
            materialFactory = null;
            renderTextureFactory = null;
            interrupted = false;
            return this;
        }

        #region Helpers
        public bool isGBufferAvailable
        {
            get { return camera.actualRenderingPath == RenderingPath.DeferredShading; }
        }

        public bool isHdr
        {
            get { return camera.allowHDR; }
        }

        public int width
        {
            get { return camera.pixelWidth; }
        }

        public int height
        {
            get { return camera.pixelHeight; }
        }

        public Rect viewport
        {
            get { return camera.rect; } // Normalized coordinates
        }
        #endregion
    }
}
