using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class SkillSlot : ContainerSlot
{
    Image icon;
    public Skill skill;
    public bool isActivated;

    private void Awake()
    {
        icon = transform.Find("Image_Item").GetComponent<Image>();
        icon.gameObject.SetActive(false);

    }

    public void Init(Skill _skill, bool _isActivated)
    {
        Clear();
        skill = _skill;
        isActivated = _isActivated;
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

        if (_skill == null)
        {

        }
        else
        {
            if (!isActivated && skill.state == SkillState.CD)
                return;
            onPointerDownEvent.AddListener(OnPointerDownEvent);
            onPointerUpEvent.AddListener(OnPointerUpEvent);
            onDragEvent.AddListener(OnDragEvent);
            icon.gameObject.SetActive(true);
            icon.sprite = Resources.Load<Sprite>(skill.skillData.icon);
        }

    }

    public void Clear()
    {
        skill = null;
        icon.sprite = null;
        icon.gameObject.SetActive(false);
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        UIController.Instance.championInfoController.OnSkillSlotDragEnd(this);
    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.championInfoController.pointEnterSlot = this;
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.championInfoController.pointEnterSlot = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
