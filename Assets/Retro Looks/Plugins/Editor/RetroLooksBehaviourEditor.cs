using System;
using System.Linq.Expressions;
using UnityEngine;
using UnityEditor;

namespace RetroLooks
{
    [CustomEditor(typeof(RetroLooksBehaviour))]
    public class RetroLooksBehaviourEditor : Editor
    {
        SerializedProperty m_Profile;

        public void OnEnable()
        {
            m_Profile = FindSetting((RetroLooksBehaviour x) => x.profile);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Profile);

            serializedObject.ApplyModifiedProperties();
        }

        SerializedProperty FindSetting<T, TValue>(Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty(ReflectionUtils.GetFieldPath(expr));
        }
    }
}
