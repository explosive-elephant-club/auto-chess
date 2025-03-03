﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseControllerUI : MonoBehaviour
{
    public bool isExpand;
    [HideInInspector]
    public CanvasGroup canvasGroup;
    // Start is called before the first frame update
    public virtual void Init()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        AutoBindingUI();
    }

    // Update is called once per frame
    public virtual void UpdateUI()
    {

    }

    public virtual void AutoBindingUI()
    {

    }

    public virtual void SetUIActive(bool isActive)
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
}
