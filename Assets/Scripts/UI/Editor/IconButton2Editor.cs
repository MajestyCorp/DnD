using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEditor;

namespace DnD.UI
{
    [CustomEditor(typeof(IconButton2))]
    public class IconButton2Editor : ButtonEditor
    {
        private SerializedProperty iconProperty;
        private SerializedProperty iconProperty2;
        private SerializedProperty dontChangeIconAlphaProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            // or any other private field
            iconProperty = serializedObject.FindProperty("icon");
            iconProperty2 = serializedObject.FindProperty("icon2");
            dontChangeIconAlphaProperty = serializedObject.FindProperty("dontChangeIconAlpha");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField(iconProperty);
            EditorGUILayout.ObjectField(iconProperty2);
            EditorGUILayout.PropertyField(dontChangeIconAlphaProperty);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}