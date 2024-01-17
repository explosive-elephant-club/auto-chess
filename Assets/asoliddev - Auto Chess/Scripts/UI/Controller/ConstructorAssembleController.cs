using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstructorAssembleController : MonoBehaviour
{
    public GameObject constructorSlotPrefab;
    public ConstructorTreeViewSlot chassisSlot;
    public ConstructorTreeViewSlot pointEnterTreeViewSlot;
    public GameObject constructorPanel;
    public Slider pitchSlider;
    public Slider yawSlider;
    public Button zoomInBtn;
    public Button zoomOutBtn;
    public Toggle editToggle;
    float[] zoomValues = { 2, 2.5f, 3, 3.5f, 4 };
    int zoomIndex = 2;
    public TextMeshProUGUI zoomValueText;
    GOToUICameraController camController;
    [HideInInspector]
    public Transform DisableSlotsParent;
    [HideInInspector]
    public Transform lineLayer;
    CanvasGroup canvasGroup;

    public ConstructorTreeViewSlot pickedSlot;

    // Start is called before the first frame update
    void Awake()
    {
        DisableSlotsParent = constructorPanel.transform.Find("DisableSlots");
        lineLayer = constructorPanel.transform.Find("LineLayer");
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        camController = GamePlayController.Instance._GOToUICameraController;
        AddAllListener();
    }

    void AddAllListener()
    {
        pitchSlider.onValueChanged.AddListener(UpdatePitchSlider);
        yawSlider.onValueChanged.AddListener(UpdateYawSlider);
        zoomInBtn.onClick.AddListener(ZoomInBtnClick);
        zoomOutBtn.onClick.AddListener(ZoomOutBtnClick);
        editToggle.onValueChanged.AddListener(editToggleClick);
    }

    void UpdatePitchSlider(float value)
    {
        camController.UpdatePitch(value);
    }
    void UpdateYawSlider(float value)
    {
        camController.UpdateYaw(value);
    }
    void ZoomInBtnClick()
    {
        if (zoomIndex < zoomValues.Length - 1)
        {
            zoomIndex++;
            camController.UpdateZoom(zoomValues[zoomIndex]);
            zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
        }
    }
    void ZoomOutBtnClick()
    {
        if (zoomIndex > 0)
        {
            zoomIndex--;
            camController.UpdateZoom(zoomValues[zoomIndex]);
            zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
        }
    }
    void editToggleClick(bool value)
    {
        constructorPanel.SetActive(value);
    }

    public void UpdateUI()
    {
        ClearAllSub(chassisSlot);
        chassisSlot.Reset();

        if (GamePlayController.Instance.ownChampionManager.pickedChampion != null)
        {
            pitchSlider.value = 0.5f;
            UpdatePitchSlider(0.5f);
            yawSlider.value = 0.5f;
            UpdateYawSlider(0.5f);
            zoomIndex = 2;
            camController.UpdateZoom(zoomValues[zoomIndex]);
            zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
            editToggle.isOn = true;
            ConstructorBase chassisConstructor = GamePlayController.Instance.ownChampionManager.pickedChampion.GetChassisConstructor();
            chassisSlot.ChassisConstructorInit(this, chassisConstructor);
            SetUIActive(true);
        }
        else
        {
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
    public void ClearAllSub(ConstructorTreeViewSlot slot)
    {
        foreach (var c in slot.children)
        {
            ClearAllSub(c);
        }
        slot.ClearSubSlot();
    }

    public GameObject NewConstructorSlot()
    {
        if (DisableSlotsParent.childCount > 0)
        {
            return DisableSlotsParent.GetChild(0).gameObject;
        }
        GameObject instance = Instantiate(constructorSlotPrefab, DisableSlotsParent);
        return instance;
    }


    public void ShowPickedSlot(ConstructorSlot slot)
    {
        chassisSlot.FindPickedSlot(slot);
        if (pickedSlot != null)
        {
            pickedSlot.pickedFrame.gameObject.SetActive(true);
        }
    }

    public void ClearPickedSlot()
    {
        if (pickedSlot != null)
        {
            pickedSlot.pickedFrame.gameObject.SetActive(false);
            pickedSlot = null;
        }
    }
}
