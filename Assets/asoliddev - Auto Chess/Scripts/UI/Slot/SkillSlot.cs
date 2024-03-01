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
    public Sprite[] levelFrames;
    public Image icon;
    public Image BG;
    public Skill skill;
    public bool isActivated;
    public SkillState skillState;

    private void Awake()
    {
    }

    private void Update()
    {
        if (skill != null)
            skillState = skill.state;
    }

    public void Init(Skill _skill, bool _isActivated)
    {
        Clear();
        skill = _skill;
        isActivated = _isActivated;
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        if (_skill != null)
        {
            if (!isActivated && skill.state != SkillState.Disable)
            {
                BG.gameObject.SetActive(false);
                return;
            }


            onPointerDownEvent.AddListener(OnPointerDownEvent);
            onPointerUpEvent.AddListener(OnPointerUpEvent);
            onDragEvent.AddListener(OnDragEvent);
            icon.gameObject.SetActive(true);
            icon.sprite = Resources.Load<Sprite>(skill.skillData.icon);

            BG.sprite = levelFrames[skill.constructor.constructorData.level - 1];
            BG.gameObject.SetActive(true);
        }
        else
        {
            BG.gameObject.SetActive(false);
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
        BG.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(true);
        BG.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        UIController.Instance.championInfoController.OnSkillSlotDragEnd(this);
    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.championInfoController.OnPointEnterSlot(this);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.championInfoController.OnPointLeaveSlot();
    }

    // Update is called once per frame
    public void PopupShow(SkillData data)
    {
        Clear();
        ClearAllListener();
        icon.gameObject.SetActive(true);
        icon.sprite = Resources.Load<Sprite>(data.icon);
    }
}
