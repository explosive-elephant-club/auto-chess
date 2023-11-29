using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class SingleTypeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Image icon;
    public ConstructorBonusType constructorBonusType;
    // Start is called before the first frame update
    private void Awake()
    {
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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.popupController.typePopup.Show
            (constructorBonusType, 1, this.gameObject, Vector3.right);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.popupController.typePopup.Clear();
    }
}
