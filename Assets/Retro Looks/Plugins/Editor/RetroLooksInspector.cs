using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RetroLooks
{
    //[CanEditMultipleObjects]
    [CustomEditor(typeof(RetroLooksProfile))]
    public class RetroLooksInspector : Editor
    {        
        RetroLooksProfile m_ConcreteTarget
        {
            get { return target as RetroLooksProfile; }
        }

        Dictionary<RetroLooksModelEditor, RetroLooksModel> m_CustomEditors = new Dictionary<RetroLooksModelEditor, RetroLooksModel>();

        public bool IsInteractivePreviewOpened { get; private set; }
        
        void OnEnable()
        {
            if (target == null)
                return;

            // Aggregate custom post-fx editors
            var assembly = Assembly.GetAssembly(typeof(RetroLooksInspector));

            var editorTypes = assembly.GetTypes()
                .Where(x => x.IsDefined(typeof(RetroLooksModelEditorAttribute), false));

            var customEditors = new Dictionary<Type, RetroLooksModelEditor>();
            foreach (var editor in editorTypes)
            {
                var attr = (RetroLooksModelEditorAttribute)editor.GetCustomAttributes(typeof(RetroLooksModelEditorAttribute), false)[0];
                var effectType = attr.type;
                var alwaysEnabled = attr.alwaysEnabled;

                var editorInst = (RetroLooksModelEditor)Activator.CreateInstance(editor);
                editorInst.alwaysEnabled = alwaysEnabled;
                editorInst.profile = target as RetroLooksProfile;
                editorInst.inspector = this;
                customEditors.Add(effectType, editorInst);
            }

            // ... and corresponding models
            var baseType = target.GetType();
            var property = serializedObject.GetIterator();

            while (property.Next(true))
            {
                if (!property.hasChildren)
                    continue;

                var type = baseType;
                var srcObject = ReflectionUtils.GetFieldValueFromPath(serializedObject.targetObject, ref type, property.propertyPath);

                if (srcObject == null)
                    continue;

                RetroLooksModelEditor editor;
                if (customEditors.TryGetValue(type, out editor))
                {
                    var effect = (RetroLooksModel)srcObject;

                    if (editor.alwaysEnabled)
                        effect.enabled = editor.alwaysEnabled;

                    m_CustomEditors.Add(editor, effect);
                    editor.target = effect;
                    editor.serializedProperty = property.Copy();
                    editor.OnPreEnable();
                }
            }
        }

        void OnDisable()
        {
            if (m_CustomEditors != null)
            {
                foreach (var editor in m_CustomEditors.Keys)
                    editor.OnDisable();

                m_CustomEditors.Clear();
            }            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Handles undo/redo events first (before they get used by the editors' widgets)
            var e = Event.current;
            if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed")
            {
                foreach (var editor in m_CustomEditors)
                    editor.Value.OnValidate();
            }
            
            foreach (var editor in m_CustomEditors)
            {
                EditorGUI.BeginChangeCheck();

                editor.Key.OnGUI();

                if (EditorGUI.EndChangeCheck())
                    editor.Value.OnValidate();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            return GraphicsUtils.supportsDX11;
        }        
    }
}
