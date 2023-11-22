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
    public CanvasGroup canvasGroup;
    public bool isNailed = false;
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        SetUIActive(false);
    }

    void Update()
    {
    }

    public virtual void Show(Vector3 slotPositon, float length, Vector3 dir)
    {
        UpdatePosition(slotPositon, length, dir);
        SetUIActive(true);
        UIController.Instance.popupController.curPickedPopup = this;
    }
    public virtual void Clear()
    {
        if (!isNailed)
        {
            SetUIActive(false);
        }
        UIController.Instance.popupController.curPickedPopup = null;
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

    public void Nail()
    {
        if (!isNailed)
        {
            UIController.Instance.popupController.nailedPopups.Add(this);
            isNailed = true;
            UIController.Instance.popupController.UpdateNailedPopupsInteract();
        }
    }

    public void Release()
    {
        if (isNailed)
        {
            UIController.Instance.popupController.nailedPopups.Remove(this);
            isNailed = false;
            SetUIActive(false);
            UIController.Instance.popupController.UpdateNailedPopupsInteract();
        }
    }
}
