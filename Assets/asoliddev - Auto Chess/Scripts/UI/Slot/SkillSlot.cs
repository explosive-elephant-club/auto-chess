using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExcelConfig;
using UnityEngine.EventSystems;
using Game;

public class SkillSlot : ContainerSlot
{
    public Skill skill;
    public bool isActivated;
    public SkillState skillState;

    #region 自动绑定
    private Image _imgPickTip;
    private Image _imgDefBG;
    private Image _imgIcon;
    private Image _imgCDMask;
    private UICustomText _textCount;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgPickTip = transform.Find("PickTip_Auto").GetComponent<Image>();
        _imgDefBG = transform.Find("DefBG_Auto").GetComponent<Image>();
        _imgIcon = transform.Find("DefBG_Auto/Mask/Icon_Auto").GetComponent<Image>();
        _imgCDMask = transform.Find("DefBG_Auto/Mask/CDMask_Auto").GetComponent<Image>();
        _textCount = transform.Find("Count_Auto").GetComponent<UICustomText>();
    }
    #endregion



    private void Update()
    {
        if (skill != null)
        {
            skillState = skill.state;
        }
        UpdateCDMask();
        UpdateCount();
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
                _imgDefBG.gameObject.SetActive(false);
                return;
            }


            onPointerDownEvent.AddListener(OnPointerDownEvent);
            onPointerUpEvent.AddListener(OnPointerUpEvent);
            onDragEvent.AddListener(OnDragEvent);
            _imgIcon.gameObject.SetActive(true);
            _imgIcon.sprite = Resources.Load<Sprite>(skill.skillData.icon);

            _imgDefBG.color = GameConfig.Instance.levelColors[skill.constructor.constructorData.level - 1];
            _imgDefBG.gameObject.SetActive(true);
        }
        else
        {
            _imgDefBG.gameObject.SetActive(false);
        }

    }

    public void ConstructorPopupInit(SkillData _skillData, ConstructorBaseData constructorBaseData)
    {
        Clear();
        ClearAllListener();
        _imgIcon.gameObject.SetActive(true);
        _imgIcon.sprite = Resources.Load<Sprite>(_skillData.icon);
        _imgDefBG.color = GameConfig.Instance.levelColors[constructorBaseData.level - 1];
        _imgDefBG.gameObject.SetActive(true);
    }

    public void UpdateCDMask()
    {
        if (skill == null)
        {
            _imgPickTip.gameObject.SetActive(false);
            _imgCDMask.fillAmount = 0;
            return;
        }
        if (GamePlayController.Instance.currentGameStage == GameStage.Combat)
        {
            if (skill.skillController.GetNextSkill() == null)
            {
                _imgPickTip.gameObject.SetActive(false);
                _imgCDMask.fillAmount = 1;
            }
            else
            {
                if (skill.skillController.GetNextSkill() == skill && skill.IsAvailable())
                {
                    _imgPickTip.gameObject.SetActive(true);
                    _imgCDMask.fillAmount = skill.skillController.cdTimer / skill.skillController.curCastDelay;
                }
                else
                {
                    _imgPickTip.gameObject.SetActive(false);
                    _imgCDMask.fillAmount = 1;
                }
            }

        }
        else
        {
            _imgPickTip.gameObject.SetActive(false);
            _imgCDMask.fillAmount = 0;
        }
    }

    public void UpdateCount()
    {
        if (skill == null || skill.skillData.usableCount == -1)
        {
            _textCount.gameObject.SetActive(false);
            return;
        }

        if (isActivated)
        {
            _textCount.gameObject.SetActive(true);
            _textCount.text = skill.countRemain.ToString();

        }
        else
        {
            if (skill.state != SkillState.Disable)
            {
                _textCount.gameObject.SetActive(false);
            }
            else
            {
                _textCount.gameObject.SetActive(true);
                _textCount.text = skill.countRemain.ToString();
            }

        }

    }

    public void Clear()
    {
        skill = null;
        _imgIcon.sprite = null;
        _imgIcon.gameObject.SetActive(false);
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        _imgIcon.gameObject.SetActive(false);
        _imgDefBG.gameObject.SetActive(false);
        draggedUI.Init(_imgIcon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        _imgIcon.gameObject.SetActive(true);
        _imgDefBG.gameObject.SetActive(true);
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
        _imgIcon.gameObject.SetActive(true);
        _imgIcon.sprite = Resources.Load<Sprite>(data.icon);
    }
}
