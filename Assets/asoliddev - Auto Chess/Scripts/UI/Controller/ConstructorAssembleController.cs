using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstructorAssembleController : BaseControllerUI
{
    public bool isEditable = true;

    public GameObject constructorSlotPrefab;
    public ConstructorTreeViewSlot chassisSlot;
    public ConstructorTreeViewSlot pointEnterTreeViewSlot;
    public GameObject constructorPanel;
    public Slider pitchSlider;
    public Slider yawSlider;
    public Button zoomInBtn;
    public Button zoomOutBtn;
    public Toggle editToggle;
    float[] zoomValues = { 6.5f, 7f, 8, 9f, 10 };
    int zoomIndex = 2;
    public TextMeshProUGUI zoomValueText;

    public GameObject expandPanel;
    public Button expandBtn;
    public Button closeBtn;

    GOToUICameraController camController;
    [HideInInspector]
    public Transform DisableSlotsParent;
    [HideInInspector]
    public Transform lineLayer;

    public ConstructorTreeViewSlot pickedSlot;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
        DisableSlotsParent = constructorPanel.transform.Find("DisableSlots");
        lineLayer = constructorPanel.transform.Find("LineLayer");
    }

    private void Start()
    {
        camController = GamePlayController.Instance._GOToUICameraController;
        AddAllListener();
        SetUIActive(false);
    }

    void AddAllListener()
    {
        pitchSlider.onValueChanged.AddListener(UpdatePitchSlider);
        yawSlider.onValueChanged.AddListener(UpdateYawSlider);
        zoomInBtn.onClick.AddListener(ZoomInBtnClick);
        zoomOutBtn.onClick.AddListener(ZoomOutBtnClick);
        editToggle.onValueChanged.AddListener(editToggleClick);
        expandBtn.onClick.AddListener(Expand);
        closeBtn.onClick.AddListener(Close);
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
        isEditable = value;
        UpdateConstructorPanel();
    }

    void Expand()
    {
        isExpand = true;
        UpdateUI();
    }

    void Close()
    {
        isExpand = false;
        UpdateUI();
    }

    public override void UpdateUI()
    {
        ClearAllSub(chassisSlot);
        chassisSlot.Reset();

        if (GamePlayController.Instance.pickedChampion != null)
        {
            if (isExpand)
            {
                expandPanel.SetActive(true);
                expandBtn.gameObject.SetActive(false);

                pitchSlider.value = 0.5f;
                UpdatePitchSlider(0.5f);
                yawSlider.value = 0.5f;
                UpdateYawSlider(0.5f);
                zoomIndex = 2;
                camController.UpdateZoom(zoomValues[zoomIndex]);
                zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
                editToggle.isOn = isEditable;
                UpdateConstructorPanel();
            }
            else
            {
                expandPanel.SetActive(false);
                expandBtn.gameObject.SetActive(true);
            }
            SetUIActive(true);
        }
        else
        {
            SetUIActive(false);
        }
    }

    void UpdateConstructorPanel()
    {
        constructorPanel.SetActive(isEditable);
        if (isEditable)
        {
            ConstructorBase chassisConstructor = GamePlayController.Instance.pickedChampion.GetChassisConstructor();
            chassisSlot.ChassisConstructorInit(this, chassisConstructor);
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
