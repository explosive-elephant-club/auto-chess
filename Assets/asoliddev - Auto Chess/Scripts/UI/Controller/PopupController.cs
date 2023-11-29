using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject popupMask;
    public SkillPopup skillPopup;
    public ConstructorPopup constructorPopup;
    public TypePopup typePopup;
    public AttributePopup attributePopup;
    public Popup curPickedPopup;
    public List<Popup> nailedPopups = new List<Popup>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(InputController.Instance.popupNailKey))
        {
            if (curPickedPopup != null)
                curPickedPopup.Nail();
        }
        if (Input.GetKeyUp(InputController.Instance.popupReleaseKey))
        {
            if (nailedPopups.Count > 0)
            {
                nailedPopups[nailedPopups.Count - 1].Release();
            }

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
