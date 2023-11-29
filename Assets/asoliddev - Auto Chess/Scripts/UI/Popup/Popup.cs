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
    RebuildAllLayout rebuildAllLayout;
    public bool isNailed = false;
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        rebuildAllLayout = gameObject.GetComponent<RebuildAllLayout>();
        SetUIActive(false);
    }

    void Update()
    {
    }

    public virtual void Show(GameObject targetUI, Vector3 dir, float length = 5f)
    {
        UpdatePosition(targetUI, dir, length);
        UIController.Instance.popupController.curPickedPopup = this;
        StartCoroutine(AsyncUpdate());
    }

    IEnumerator AsyncUpdate()
    {
        yield return StartCoroutine(rebuildAllLayout.RebuildAllSizeFitterRects());
        SetUIActive(true);
    }

    public virtual void Clear()
    {
        StopAllCoroutines();
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

    public void UpdatePosition(GameObject targetUI, Vector3 dir, float length)
    {
        float selfLength = 0;
        float targetLength = 0;
        if (dir.x != 0)
        {
            selfLength = GetComponent<RectTransform>().rect.width;
            targetLength = targetUI.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            selfLength = GetComponent<RectTransform>().rect.height;
            targetLength = targetUI.GetComponent<RectTransform>().rect.height;
        }
        float offset = (targetLength + selfLength) + length;
        transform.position = targetUI.transform.position + dir.normalized * offset;
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
