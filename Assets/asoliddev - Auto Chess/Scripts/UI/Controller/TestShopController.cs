using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestShopController : BaseControllerUI
{
    CanvasGroup canvasGroup;
    public GameObject testBuyBtn;
    public Transform content;

    public Scrollbar scrollbar;

    public InputField inputField;
    public Button searchBtn;


    List<TestShopConstructBtn> testShopConstructBtns;

    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        searchBtn.onClick.AddListener(Search);
        SetUIActive(false);
    }
    
    #region 自动绑定
    
    #endregion
    void Start()
    {
        testShopConstructBtns = new List<TestShopConstructBtn>();
        foreach (var c in GameExcelConfig.Instance.constructorsArray)
        {
            if (c.type != "Isolate")
            {
                TestShopConstructBtn btn = Instantiate(testBuyBtn).GetComponent<TestShopConstructBtn>();
                btn.transform.SetParent(content, false);
                btn.Refresh(c);
                testShopConstructBtns.Add(btn);
            }

        }
    }

    private void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.F5))
        {
            SetUIActive(!canvasGroup.interactable);
        }*/
    }

    // Update is called once per frame
    void Search()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            int id = int.Parse(inputField.text);
            int index = testShopConstructBtns.FindIndex(c => c.constructorData.ID == id);

            int denominator = (int)Mathf.Ceil(testShopConstructBtns.Count / 3);
            int numerator = (int)Mathf.Floor(index / 3);
            scrollbar.value = 1 - ((float)numerator / (float)denominator);
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
}
