using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructorAssembleController : MonoBehaviour
{
    public GameObject constructorSlotPrefab;
    public ConstructorTreeViewSlot baseSlot;
    public ConstructorTreeViewSlot pointEnterInventorySlot;
    Transform DisableSlotsParent;
    CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Awake()
    {
        DisableSlotsParent = transform.Find("Tab/DisableSlots");
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }


    public void UpdateUI()
    {
        if (GamePlayController.Instance.ownChampionManager.pickedChampion != null)
        {
            ConstructorBase constructorBase = GamePlayController.Instance.ownChampionManager.pickedChampion.GetBaseTypeConstructor();
            baseSlot.Init(this, constructorBase);
            SetUIActive(true);
        }
        else
        {
            baseSlot.Clear();
            Clear(baseSlot);
            SetUIActive(false);
        }
    }

    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Clear(ConstructorTreeViewSlot slot)
    {
        foreach (var c in slot.children)
        {
            Clear(c);
        }
        slot.ClearSubSlot();
    }

    public ConstructorTreeViewSlot NewConstructorSlot()
    {
        if (DisableSlotsParent.childCount > 0)
        {
            return DisableSlotsParent.GetChild(0).GetComponent<ConstructorTreeViewSlot>();
        }
        GameObject instance = Instantiate(constructorSlotPrefab, DisableSlotsParent);
        return instance.GetComponent<ConstructorTreeViewSlot>();
    }

    public void RecyclingConstructorSlot(ConstructorTreeViewSlot slot)
    {
        slot.transform.parent = DisableSlotsParent;
    }

    public IEnumerator AllLayoutRebuilder(ConstructorTreeViewSlot slot)
    {
        StartCoroutine(slot.UpdateRectSize());

        yield return 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform.Find("Tab").GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }

}
