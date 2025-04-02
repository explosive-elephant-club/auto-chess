using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(UICustomText), true)]
[CanEditMultipleObjects]
public class UICustomTextInspector : UnityEditor.UI.TextEditor
{
    SerializedProperty _isUseMaxWidth;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        _isUseMaxWidth = serializedObject.FindProperty("isUseMaxWidth");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_isUseMaxWidth);
        if (_isUseMaxWidth.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxWidth"));
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlinePerFontSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowPerFontSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isUseLink"));
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
