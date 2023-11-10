using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;
using UnityEditor;

public class ConstructorTreeViewSlot : ContainerSlot
{
    Image icon;
    TextMeshProUGUI text;
    Toggle expandToggle;
    public Transform subTab;

    ConstructorAssembleController controller;

    public Sprite emptyIcon;
    public Sprite constructorIcon;

    public ConstructorBase constructor;
    public List<ConstructorTreeViewSlot> children = new List<ConstructorTreeViewSlot>();

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("ConstructorInfo/Icon").GetComponent<Image>();
        emptyIcon = icon.sprite;
        text = transform.Find("ConstructorInfo/TypeText").GetComponent<TextMeshProUGUI>();
        expandToggle = transform.Find("ConstructorInfo/Toggle").GetComponent<Toggle>();
        subTab = transform.Find("SubTab");
    }

    public void Init(ConstructorAssembleController _controller, ConstructorSlot _constructorSlot)
    {
        controller = _controller;
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

        string str = "";
        foreach (var t in _constructorSlot.adaptTypes)
        {
            str += (t.ToString() + "//");
        }
        str.Remove(str.Length - 3, str.Length - 1);
        text.text = str;
        expandToggle.gameObject.SetActive(false);
    }

    public void Init(ConstructorAssembleController _controller, ConstructorBase _constructor)
    {
        controller = _controller;
        constructor = _constructor;
        ClearAllListener();
        expandToggle.onValueChanged.RemoveAllListeners();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        onPointerDownEvent.AddListener(OnPointerDownEvent);
        onPointerUpEvent.AddListener(OnPointerUpEvent);
        onDragEvent.AddListener(OnDragEvent);

        StartCoroutine(LoadIcon());
        text.text = _constructor.type.ToString();
        expandToggle.gameObject.SetActive(true);
        expandToggle.onValueChanged.AddListener((bool var) =>
        {
            if (var)
                ExpandSubSlot();
            else
                ClearSubSlot();
        });
    }

    public IEnumerator LoadIcon()
    {

        yield return new WaitUntil(() => AssetPreview.GetAssetPreview(constructor.gameObject) != null);
        Texture2D tex = AssetPreview.GetAssetPreview(constructor.gameObject);
        constructorIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        icon.sprite = constructorIcon;
        yield return 0;
    }

    public void ExpandSubSlot()
    {
        foreach (var s in constructor.slots)
        {
            ConstructorTreeViewSlot treeViewSlot = controller.NewConstructorSlot();
            if (s.constructorInstance == null)
            {
                treeViewSlot.Init(controller, s);
            }
            else
            {
                treeViewSlot.Init(controller, s.constructorInstance);
            }
            treeViewSlot.transform.parent = subTab;
            children.Add(treeViewSlot);
        }
        controller.AllLayoutRebuilder(this);
    }

    public void ClearSubSlot()
    {
        for (int i = 0; i < children.Count; i++)
        {
            controller.RecyclingConstructorSlot(children[i]);
        }
        if (controller != null)
            controller.AllLayoutRebuilder(this);
    }


    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.sprite = emptyIcon;
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.sprite = constructorIcon;
        draggedUI.OnPointerUp(eventData);

    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {

    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {

    }
}
