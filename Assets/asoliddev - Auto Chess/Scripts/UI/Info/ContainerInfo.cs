using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using UnityEngine.EventSystems;

public class ContainerInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public CanvasGroup canvasGroup;
    public PointerEvent onPointerEnterEvent = new PointerEvent();
    public PointerEvent onPointerExitEvent = new PointerEvent();

    public virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        AutoBindingUI();
    }

    public void ClearAllListener()
    {
        onPointerEnterEvent.RemoveAllListeners();
        onPointerExitEvent.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnterEvent.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExitEvent.Invoke(eventData);
    }
    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

    }

    public virtual void AutoBindingUI()
    {

    }
}
