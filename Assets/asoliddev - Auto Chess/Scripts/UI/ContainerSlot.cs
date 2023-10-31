using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class PointerEvent : UnityEvent<PointerEventData> { }

public class ContainerSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IDragHandler
{
    public PointerEvent onPointerDownEvent = new PointerEvent();
    public PointerEvent onPointerUpEvent = new PointerEvent();
    public PointerEvent onPointerEnterEvent = new PointerEvent();
    public PointerEvent onDragEvent = new PointerEvent();
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
