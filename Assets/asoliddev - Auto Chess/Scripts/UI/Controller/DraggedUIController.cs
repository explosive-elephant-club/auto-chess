using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DraggedUIController : MonoBehaviour
{
    public Image icon;
    Vector3 offset;
    CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        SetUIActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(Sprite _icon, GameObject ui)
    {
        icon.sprite = _icon;
        Vector2 size = ui.GetComponent<RectTransform>().sizeDelta;
        //float min = size.x > size.y ? size.y : size.x;

        //GetComponent<RectTransform>().sizeDelta = new Vector2(min, min);
        SetUIActive(true);
    }

    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            //canvasGroup.interactable = true;
            //canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            //canvasGroup.interactable = false;
            //canvasGroup.blocksRaycasts = false;
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = new Vector3(5, 5, 0);
        //Vector2 mousePos = Mouse.current.position.ReadValue();
        //offset = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        transform.position = new Vector3(mousePos.x, mousePos.y, 0) - offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetUIActive(false);
    }
}
