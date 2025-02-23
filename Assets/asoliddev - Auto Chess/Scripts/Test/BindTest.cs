using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BindTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    #region 自动绑定
    private Image _imgTest;
    private Text _textTest;
    #endregion
    //自动获取组件添加字典管理
    private void AutoBindingUI()
    {
        _imgTest = transform.Find("Test_Auto").GetComponent<Image>();
        _textTest = transform.Find("Test_Auto").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
