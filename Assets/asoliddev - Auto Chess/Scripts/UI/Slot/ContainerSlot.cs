using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class PointerEvent : UnityEvent<PointerEventData> { }

public class ContainerSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    public PointerEvent onPointerDownEvent = new PointerEvent();
    public PointerEvent onPointerUpEvent = new PointerEvent();
    public PointerEvent onPointerEnterEvent = new PointerEvent();
    public PointerEvent onPointerExitEvent = new PointerEvent();
    public PointerEvent onDragEvent = new PointerEvent();
    protected DraggedUIController draggedUI;
    void Start()
    {
        draggedUI = GameObject.Find("ScreenCanvas/DraggedUI").GetComponent<DraggedUIController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClearAllListener()
    {
        onPointerDownEvent.RemoveAllListeners();
        onDragEvent.RemoveAllListeners();
        onPointerUpEvent.RemoveAllListeners();
        onPointerEnterEvent.RemoveAllListeners();
        onPointerExitEvent.RemoveAllListeners();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDownEvent.Invoke(eventData);
       
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDragEvent.Invoke(eventData);

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUpEvent.Invoke(eventData);
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnterEvent.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExitEvent.Invoke(eventData);
    }
}
