using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using UnityEngine.EventSystems;

public class SingleMFInfo : ContainerInfo
{
    Image icon;
    public ConstructorBonusType constructorBonusType;
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        icon = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(ConstructorBonusType _constructorBonusType)
    {
        constructorBonusType = _constructorBonusType;
        icon.sprite = Resources.Load<Sprite>(constructorBonusType.icon);
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.typePopup.Show
            (constructorBonusType, 1, this.gameObject, Vector3.right);

    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.typePopup.Clear();
    }
}
