using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopupController : BaseControllerUI
{
    public GameObject popupMask;
    public SkillPopup skillPopup;
    public ConstructorPopup constructorPopup;
    public ManufacturerPopup manufacturerPopup;
    public AttributePopup attributePopup;
    public SlotPopup constructorSlotPopup;
    public Popup curPickedPopup;
    public List<Popup> nailedPopups = new List<Popup>();

    #region 自动绑定

    #endregion

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        constructorPopup = ResourceManager.LoadGameObjectResource("UI/Popup/ConstructorPopup", transform).GetComponent<ConstructorPopup>();
        manufacturerPopup = ResourceManager.LoadGameObjectResource("UI/Popup/ManufacturerPopup", transform).GetComponent<ManufacturerPopup>();
        attributePopup = ResourceManager.LoadGameObjectResource("UI/Popup/AttributePopup", transform).GetComponent<AttributePopup>();
        skillPopup = ResourceManager.LoadGameObjectResource("UI/Popup/SkillPopup", transform).GetComponent<SkillPopup>();
        constructorSlotPopup = ResourceManager.LoadGameObjectResource("UI/Popup/SlotPopup", transform).GetComponent<SlotPopup>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PopupNail(InputAction.CallbackContext context)
    {
        if (context.started || curPickedPopup != null)
            curPickedPopup.Nail();
    }

    public void PopupUnnail(InputAction.CallbackContext context)
    {
        if (context.started)
            if (nailedPopups.Count > 0)
            {
                nailedPopups[nailedPopups.Count - 1].Release();
            }
    }

    public void UpdateNailedPopupsInteract()
    {
        if (nailedPopups.Count > 0)
        {
            foreach (var popup in nailedPopups)
            {
                popup.canvasGroup.interactable = false;
                popup.canvasGroup.blocksRaycasts = false;
            }
            nailedPopups[nailedPopups.Count - 1].canvasGroup.interactable = true;
            nailedPopups[nailedPopups.Count - 1].canvasGroup.blocksRaycasts = true;
            popupMask.SetActive(true);
        }
        else
        {
            popupMask.SetActive(false);
        }

    }
}
