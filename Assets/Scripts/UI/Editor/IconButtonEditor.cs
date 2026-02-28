using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEditor;

namespace DnD.UI
{
    [CustomEditor(typeof(IconButton))]
    public class IconButtonEditor : ButtonEditor
    {
        private SerializedProperty iconProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            // or any other private field
            iconProperty = serializedObject.FindProperty("icon");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField(iconProperty);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}