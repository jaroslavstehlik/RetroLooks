using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

namespace RetroLooks
{
    [ImageEffectAllowedInSceneView]
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent, ExecuteInEditMode]
    [AddComponentMenu("Effects/RetroLooks Behaviour", -1)]
    public class RetroLooksBehaviour : MonoBehaviour
    {
        // Inspector fields
        public RetroLooksProfile profile;

        public Func<Vector2, Matrix4x4> jitteredMatrixFunc;

        // Internal helpers
        Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>> m_CommandBuffers;
        List<RetroLooksComponentBase> m_Components;
        Dictionary<RetroLooksComponentBase, bool> m_ComponentStates;

        MaterialFactory m_MaterialFactory;
        RenderTextureFactory m_RenderTextureFactory;
        RetroLooksContext m_Context;
        Camera m_Camera;
        RetroLooksProfile m_PreviousProfile;

        bool m_RenderingInSceneView = false;

        // Effect components
        RetroBloomComponent m_RetroBloom;

        void OnEnable()
        {
            m_CommandBuffers = new Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>>();
            m_MaterialFactory = new MaterialFactory();
            m_RenderTextureFactory = new RenderTextureFactory();
            m_Context = new RetroLooksContext();

            // Keep a list of all post-fx for automation purposes
            m_Components = new List<RetroLooksComponentBase>();

            // Component list
            m_RetroBloom = AddComponent(new RetroBloomComponent());

            // Prepare state observers
            m_ComponentStates = new Dictionary<RetroLooksComponentBase, bool>();

            foreach (var component in m_Components)
                m_ComponentStates.Add(component, false);

            useGUILayout = false;
        }

        void OnPreCull()
        {
            // All the per-frame initialization logic has to be done in OnPreCull instead of Update
            // because [ImageEffectAllowedInSceneView] doesn't trigger Update events...

            m_Camera = GetComponent<Camera>();

            if (profile == null || m_Camera == null)
                return;

#if UNITY_EDITOR
            // Track the scene view camera to disable some effects we don't want to see in the
            // scene view
            // Currently disabled effects :
            //  - Temporal Antialiasing
            //  - Depth of Field
            //  - Motion blur
            m_RenderingInSceneView = UnityEditor.SceneView.currentDrawingSceneView != null
                && UnityEditor.SceneView.currentDrawingSceneView.camera == m_Camera;
#endif

            // Prepare context
            var context = m_Context.Reset();
            context.profile = profile;
            context.renderTextureFactory = m_RenderTextureFactory;
            context.materialFactory = m_MaterialFactory;
            context.camera = m_Camera;

            // Prepare components
            m_RetroBloom.Init(context, profile.retroBloom);

            // Handles profile change and 'enable' state observers
            if (m_PreviousProfile != profile)
            {
                DisableComponents();
                m_PreviousProfile = profile;
            }

            CheckObservers();

            // Find out which camera flags are needed before rendering begins
            // Note that motion vectors will only be available one frame after being enabled
            var flags = DepthTextureMode.None;
            foreach (var component in m_Components)
            {
                if (component.active)
                    flags |= component.GetCameraFlags();
            }

            context.camera.depthTextureMode = flags;            
        }

        void OnPreRender()
        {
            if (profile == null)
                return;            
        }

        void OnPostRender()
        {
            if (profile == null || m_Camera == null)
                return;            
        }

        // Classic render target pipeline for RT-based effects
        // Note that any effect that happens after this stack will work in LDR
        [ImageEffectTransformsToLDR]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (profile == null || m_Camera == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            
            var src = source;
            var dst = destination;

            var retroMaterial = m_MaterialFactory.Get("Retro Looks/Filters/RetroLooks");
            retroMaterial.shaderKeywords = null;

            if (m_RetroBloom.active)
            {
                m_RetroBloom.Prepare(src, retroMaterial);
                Graphics.Blit(src, dst, retroMaterial);
            }
            else
            {
                Graphics.Blit(src, dst);
            }

            m_RenderTextureFactory.ReleaseAll();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (profile == null || m_Camera == null)
                return;            
        }

        void OnDisable()
        {
            // Clear command buffers
            foreach (var cb in m_CommandBuffers.Values)
            {
                m_Camera.RemoveCommandBuffer(cb.Key, cb.Value);
                cb.Value.Dispose();
            }

            m_CommandBuffers.Clear();

            // Clear components
            if (profile != null)
                DisableComponents();

            m_Components.Clear();

            // Reset camera mode
            if (m_Camera != null)
                m_Camera.depthTextureMode = DepthTextureMode.None;

            // Factories
            m_MaterialFactory.Dispose();
            m_RenderTextureFactory.Dispose();
            GraphicsUtils.Dispose();
        }
        
        #region State management

        List<RetroLooksComponentBase> m_ComponentsToEnable = new List<RetroLooksComponentBase>();
        List<RetroLooksComponentBase> m_ComponentsToDisable = new List<RetroLooksComponentBase>();

        void CheckObservers()
        {
            foreach (var cs in m_ComponentStates)
            {
                var component = cs.Key;
                var state = component.GetModel().enabled;

                if (state != cs.Value)
                {
                    if (state) m_ComponentsToEnable.Add(component);
                    else m_ComponentsToDisable.Add(component);
                }
            }

            for (int i = 0; i < m_ComponentsToDisable.Count; i++)
            {
                var c = m_ComponentsToDisable[i];
                m_ComponentStates[c] = false;
                c.OnDisable();
            }

            for (int i = 0; i < m_ComponentsToEnable.Count; i++)
            {
                var c = m_ComponentsToEnable[i];
                m_ComponentStates[c] = true;
                c.OnEnable();
            }

            m_ComponentsToDisable.Clear();
            m_ComponentsToEnable.Clear();
        }

        void DisableComponents()
        {
            foreach (var component in m_Components)
            {
                var model = component.GetModel();
                if (model != null && model.enabled)
                    component.OnDisable();
            }
        }

        #endregion

        #region Command buffer handling & rendering helpers
        // Placeholders before the upcoming Scriptable Render Loop as command buffers will be
        // executed on the go so we won't need of all that stuff
        CommandBuffer AddCommandBuffer<T>(CameraEvent evt, string name)
            where T : RetroLooksModel
        {
            var cb = new CommandBuffer { name = name };
            var kvp = new KeyValuePair<CameraEvent, CommandBuffer>(evt, cb);
            m_CommandBuffers.Add(typeof(T), kvp);
            m_Camera.AddCommandBuffer(evt, kvp.Value);
            return kvp.Value;
        }

        void RemoveCommandBuffer<T>()
            where T : RetroLooksModel
        {
            KeyValuePair<CameraEvent, CommandBuffer> kvp;
            var type = typeof(T);

            if (!m_CommandBuffers.TryGetValue(type, out kvp))
                return;

            m_Camera.RemoveCommandBuffer(kvp.Key, kvp.Value);
            m_CommandBuffers.Remove(type);
            kvp.Value.Dispose();
        }

        CommandBuffer GetCommandBuffer<T>(CameraEvent evt, string name)
            where T : RetroLooksModel
        {
            CommandBuffer cb;
            KeyValuePair<CameraEvent, CommandBuffer> kvp;

            if (!m_CommandBuffers.TryGetValue(typeof(T), out kvp))
            {
                cb = AddCommandBuffer<T>(evt, name);
            }
            else if (kvp.Key != evt)
            {
                RemoveCommandBuffer<T>();
                cb = AddCommandBuffer<T>(evt, name);
            }
            else cb = kvp.Value;

            return cb;
        }

        void TryExecuteCommandBuffer<T>(RetroLooksComponentCommandBuffer<T> component)
            where T : RetroLooksModel
        {
            if (component.active)
            {
                var cb = GetCommandBuffer<T>(component.GetCameraEvent(), component.GetName());
                cb.Clear();
                component.PopulateCommandBuffer(cb);
            }
            else RemoveCommandBuffer<T>();
        }

        bool TryPrepareUberImageEffect<T>(RetroLooksComponentRenderTexture<T> component, Material material)
            where T : RetroLooksModel
        {
            if (!component.active)
                return false;

            component.Prepare(material);
            return true;
        }

        T AddComponent<T>(T component)
            where T : RetroLooksComponentBase
        {
            m_Components.Add(component);
            return component;
        }

        #endregion
    }
}
