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

    DraggedUIController draggedUI;

    private void Awake()
    {
        icon = transform.Find("Image_Item").GetComponent<Image>();
        icon.gameObject.SetActive(false);
        draggedUI = GameObject.Find("ScreenCanvas/DraggedUI").GetComponent<DraggedUIController>();
    }

    public void Init(Skill _skill, bool _isActivated)
    {
        skill = _skill;
        isActivated = _isActivated;
        icon.gameObject.SetActive(true);
        icon.sprite = Resources.Load<Sprite>(skill.skillData.icon);
    }

    public void Clear()
    {
        skill = null;
        icon.sprite = null;
        icon.gameObject.SetActive(false);
    }

    public void AddActivatedSkill()
    {

    }
    public void AddDeactivatedSkill()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
