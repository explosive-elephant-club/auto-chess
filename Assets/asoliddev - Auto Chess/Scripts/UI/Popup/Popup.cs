using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

[Serializable]
public class TextPair
{
    public Text attribute;
    public Text value;
}

public class Popup : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    RebuildAllLayout rebuildAllLayout;
    public bool isNailed = false;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rebuildAllLayout = GetComponent<RebuildAllLayout>();
        rectTransform = GetComponent<RectTransform>();
        SetUIActive(false);
    }

    void Update()
    {
    }

    public virtual void Show(GameObject targetUI, Vector2 dir, float length = 5f)
    {
        UIController.Instance.popupController.curPickedPopup = this;
        StartCoroutine(AsyncUpdate(targetUI, dir, length));
    }

    IEnumerator AsyncUpdate(GameObject targetUI, Vector2 dir, float length = 5f)
    {
        yield return StartCoroutine(rebuildAllLayout.RebuildAllSizeFitterRects());
        UpdatePosition(targetUI, dir, length);
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

    public void UpdatePosition(GameObject targetUI, Vector2 dir, float length)
    {
        float selfLength = 0;
        float targetLength = 0;
        if (dir.x != 0)
        {
            selfLength = rectTransform.rect.width;
            targetLength = targetUI.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            selfLength = rectTransform.rect.height;
            targetLength = targetUI.GetComponent<RectTransform>().rect.height;
        }
        float offset = (targetLength + selfLength) / 2 + length;
        transform.position = targetUI.transform.position;
        rectTransform.anchoredPosition = rectTransform.anchoredPosition + dir.normalized * offset;
        LockOnScreen();
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

    public void LockOnScreen()
    {
        float topDis = Mathf.Abs(rectTransform.anchoredPosition.y) - rectTransform.rect.height / 2;
        float bottomDis = transform.parent.GetComponent<RectTransform>().rect.height -
                            Mathf.Abs(rectTransform.anchoredPosition.y) -
                                rectTransform.rect.height / 2;
        float leftDis = Mathf.Abs(rectTransform.anchoredPosition.x) - rectTransform.rect.width / 2;
        float rightDis = transform.parent.GetComponent<RectTransform>().rect.width -
                            Mathf.Abs(rectTransform.anchoredPosition.x) -
                                rectTransform.rect.width / 2;

        if (topDis < 0)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0, topDis);
        }
        else if (bottomDis < 0)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0, -bottomDis);
        }

        if (leftDis < 0)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(-leftDis, 0);
        }
        else if (rightDis < 0)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(rightDis, 0);
        }
    }
}
