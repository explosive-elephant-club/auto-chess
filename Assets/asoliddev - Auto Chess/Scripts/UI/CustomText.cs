using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEditor;

[ExecuteAlways]
public class CustomText : MonoBehaviour
{
    int fSize = 8;

    Text text;
    Shadow shadow;
    Outline outline;

    void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        text = GetComponent<Text>();
        shadow = GetComponent<Shadow>();
        outline = GetComponent<Outline>();
        fSize = text.fontSize;
    }

#if UNITY_EDITOR

    private void EditorUpdate()
    {
        if (fSize != text.fontSize)
        {
            fSize = text.fontSize;
            UpdateTextEffect();
        }
    }
#endif
    void Update()
    {
        if (fSize != text.fontSize)
        {
            fSize = text.fontSize;
            UpdateTextEffect();
        }
    }

    void UpdateTextEffect()
    {
        float proportion = (float)fSize / 12;
        if (outline != null)
        {
            outline.effectDistance = new Vector2(proportion, proportion * -1);
        }
        if (shadow != null)
        {
            shadow.effectDistance = new Vector2(proportion, proportion * -1);
        }
    }
}
