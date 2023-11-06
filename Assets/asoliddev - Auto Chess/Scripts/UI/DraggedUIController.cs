using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class DraggedUIController : MonoBehaviour
{
    public Image icon;
    Vector3 offset;

    UnityAction OnDragEnd;

    CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        OnDragEnd = new UnityAction(() => { });
        SetUIActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(Sprite _icon, UnityAction onDragEnd)
    {
        icon.sprite = _icon;
        OnDragEnd = onDragEnd;
        SetUIActive(true);
    }

    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            //canvasGroup.interactable = true;
            //canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            //canvasGroup.interactable = false;
            //canvasGroup.blocksRaycasts = false;
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = Input.mousePosition - transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition - offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //OnDragEnd.Invoke();
        SetUIActive(false);
    }
}
