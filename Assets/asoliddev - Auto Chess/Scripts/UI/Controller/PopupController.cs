using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopupController : BaseControllerUI
{
    public GameObject popupMask;
    public SkillPopup skillPopup;
    public ConstructorPopup constructorPopup;
    public TypePopup typePopup;
    public AttributePopup attributePopup;
    public ConstructorSlotPopup constructorSlotPopup;
    public Popup curPickedPopup;
    public List<Popup> nailedPopups = new List<Popup>();
    
    #region 自动绑定
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {

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
