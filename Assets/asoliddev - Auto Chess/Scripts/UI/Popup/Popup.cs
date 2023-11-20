using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

[Serializable]
public class TextPair
{
    public TextMeshProUGUI attribute;
    public TextMeshProUGUI value;
}

public class Popup : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    public Transform parent;
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        SetUIActive(false);
    }
    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void UpdatePosition(Vector3 slotPositon, float length, Vector3 dir)
    {

        float selfLength = 0;
        if (dir.x != 0)
        {
            selfLength = GetComponent<RectTransform>().rect.width;
        }
        else
        {
            selfLength = GetComponent<RectTransform>().rect.height;
        }
        float offset = (length + selfLength) / 2 + 5;
        transform.position = slotPositon + dir.normalized * offset;
    }
}
