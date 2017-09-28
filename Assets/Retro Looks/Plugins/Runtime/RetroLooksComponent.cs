using UnityEngine.Rendering;
using UnityEngine;

namespace RetroLooks
{
    public abstract class RetroLooksComponentBase
    {
        public RetroLooksContext context;

        public virtual DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.None;
        }

        public abstract bool active { get; }

        public virtual void OnEnable()
        {}

        public virtual void OnDisable()
        {}

        public abstract RetroLooksModel GetModel();
    }

    public abstract class RetroLooksComponent<T> : RetroLooksComponentBase
        where T : RetroLooksModel
    {
        public T model { get; internal set; }

        public virtual void Init(RetroLooksContext pcontext, T pmodel)
        {
            context = pcontext;
            model = pmodel;
        }

        public override RetroLooksModel GetModel()
        {
            return model;
        }
    }

    public abstract class RetroLooksComponentCommandBuffer<T> : RetroLooksComponent<T>
        where T : RetroLooksModel
    {
        public abstract CameraEvent GetCameraEvent();

        public abstract string GetName();

        public abstract void PopulateCommandBuffer(CommandBuffer cb);
    }

    public abstract class RetroLooksComponentRenderTexture<T> : RetroLooksComponent<T>
        where T : RetroLooksModel
    {
        public virtual void Prepare(Material material)
        {}
    }
}
