using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using UnityEngine.EventSystems;

public class SlotInfo : ContainerInfo
{
    ConstructorSlot slot;
    ConstructorSlotType slotTypeData;
    #region 自动绑定
	private Image _imgIcon;
	private Image _imgForbidden;
	//自动获取组件添加字典管理
	public override void AutoBindingUI()
	{
		_imgIcon = transform.Find("Mask/Icon_Auto").GetComponent<Image>();
		_imgForbidden = transform.Find("Mask/Forbidden_Auto").GetComponent<Image>();
	}
	#endregion



    public void Init(int id)
    {
        slot = null;
        slotTypeData = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == id);
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        _imgForbidden.enabled = false;
    }
    public void Init(ConstructorSlot _slot)
    {
        slot = _slot;
        slotTypeData = slot.slotType;

        _imgForbidden.enabled = false;
        if (slot != null)
        {
            if (!slot.isAble)
            {
                _imgForbidden.enabled = true;
            }
        }
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Show
            (slotTypeData, this.gameObject, Vector3.right);
        if (slot != null)
            UIController.Instance.constructorAssembleController.ShowPickedSlotFrame(slot);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Clear();
        if (slot != null)
            UIController.Instance.constructorAssembleController.ClosePickedSlotFrame();
    }
}
