using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using Game;

public class UIBindTool : Editor
{
    private static string endWith = "_Auto";
    // 定义正则表达式模式
    private static string pattern = @"#region 自动绑定\s*(.*?)\s*#endregion";
    /// <summary>
    /// 简称+真实地址  类型
    /// </summary>
    private static Dictionary<string, Type> findDic = new Dictionary<string, Type>();//保存已经找到的控件 类型名字简称+真实地址 类型 原因防止key重复并且都是3位好处理
    private static Dictionary<string, Type> realTypeDic = new Dictionary<string, Type>();//保存已经找到的控件 类型名字简称+真实地址 类型 原因防止key重复并且都是3位好处理
    private static Dictionary<Type, string> dicMap = GenerateBtuDic();
    [MenuItem("GameObject/GenerateBinding", false, 0)]
    private static void GenerateBinding()
    {
        findDic.Clear();
        realTypeDic.Clear();
        if (Selection.gameObjects.Length < 1)//返回当前在Hierarchy面板上或Project文件夹下选择的游戏物体GameObject数组，未选择则数组长度则为0
            return;
        GameObject selectObj = Selection.gameObjects[0];//找到选中的第一个物体
                                                        //foreach (var item in Selection.gameObjects)
                                                        //{
                                                        //    Debug.Log(item.name);
                                                        //}
                                                        //if (dicMap == null) ;
                                                        //Dictionary<Type, string> dicMap = GenerateBtuDic();//控件类型对应的名字字典


        StringBuilder pasteContent = new StringBuilder();
        //声明保存的字典 
        //rider不要\t
        // pasteContent.Append("\t#region 自动绑定\n");
        pasteContent.Append("#region 自动绑定\n");
        // pasteContent.Append("\tprivate Dictionary<string, UIBehaviour> componentsDic = new Dictionary<string, UIBehaviour>();\n");
        foreach (Type type in dicMap.Keys)//看看物体上有没有对应类型的控件 keys是UI组件类型
        {
            //获取子物体上所有对应UI组件
            Component[] components = selectObj.GetComponentsInChildren(type);//GetComponentsInChildren可以获取到儿子孙子 深度优先 包含自己
            if (components.Length < 1) continue;//没有获取到就下一个
            for (int i = 0; i < components.Length; i++)
            {
                //if (!components[i].name.StartsWith(startsWith))
                if (!components[i].name.EndsWith(endWith))
                    continue;// 加一个字符串条件判断

                //满足条件
                Component component = components[i];

                //组件的名字
                string name = dicMap[type] + component.name.Substring(0, component.name.Length - endWith.Length);
                //组件的地址 
                string path = component.name;
                Transform transform = component.transform;
                //构成控件的path
                bool needSkip = false;
                while (!transform.parent.name.Equals(selectObj.name))//中间的地址
                {
                    //Debug.Log(transform.name);
                    path = transform.parent.name + "/" + path;
                    transform = transform.parent;
                    if (transform.GetComponent<SubViewBase>() != null ||
                        transform.GetComponent<ContainerSlot>() != null)
                    {
                        needSkip = true;
                        break;
                    }
                }
                if (needSkip) continue;

                //地址=父物体名字+中间地址+名字
                //Debug.Log(selectObj.name +"/"+ path);
                findDic.Add(dicMap[type] + /*selectObj.name + "/" +*/ path, type);
                realTypeDic.Add(dicMap[type] + /*selectObj.name + "/" +*/ path, component.GetType());
                //Debug.Log(dicMap[type] + /*selectObj.name + "/" +*/ path);
                //组件的声明
                pasteContent.Append("\t" + "private" + " " + component.GetType().Name + " _" + name + ";");
                pasteContent.Append("\n");

            }
            //Debug.Log(pasteContent);

        }
        //查找组件
        pasteContent.Append("\t//自动获取组件添加字典管理");
        pasteContent.Append("\n");
        pasteContent.Append("\tpublic override void AutoBindingUI()");
        pasteContent.Append("\n");
        pasteContent.Append("\t{");
        foreach (string item in findDic.Keys)
        {
            //!!!直接在选中物体下面的会有Bug!!!  如果一共就一位 先去掉前缀再继续
            string[] temp = item.Split("/");
            string componentName;
            if (temp.Length > 1)
                //获取真实名字
                componentName = temp[temp.Length - 1];
            else
                componentName = temp[temp.Length - 1].Substring(dicMap[findDic[item]].Length);


            //获取真实地址 Bug
            string realPath = item.Substring(dicMap[findDic[item]].Length);


            //拼接变量名
            string varName = dicMap[findDic[item]] + componentName.Substring(0, componentName.Length - endWith.Length);
            //Debug.Log(componentName.Substring(0, componentName.Length - endWith.Length));
            //Debug.Log(componentName + " " + realPath + " " + varName);
            //语句
            pasteContent.Append("\n");
            pasteContent.Append($"\t\t_{varName} = transform.Find(\"{realPath}\").GetComponent<{realTypeDic[item].Name}>();");
            //rigidbody = transform.Find().GetComponent<>();

        }
        /*//加入字典
        foreach (string item in findDic.Keys)
        {
            //!!!直接在选中物体下面的会有Bug!!!  如果一共就一位 先去掉前缀再继续
 
            string[] temp = item.Split("/");
            string componentName;
            if (temp.Length > 1)
                //获取真实名字
                componentName = temp[temp.Length - 1];
            else
                componentName = temp[temp.Length - 1].Substring(dicMap[findDic[item]].Length);
            //拼接变量名
            string varName = dicMap[findDic[item]] + componentName.Substring(0, componentName.Length - endWith.Length);
            //Debug.Log(componentName.Substring(0, componentName.Length - endWith.Length));
            //Debug.Log(componentName + " " + realPath + " " + varName);
            //语句
            pasteContent.Append("\n");
            pasteContent.Append($"\t\tcomponentsDic.Add(\"{varName.ToString()}\", {varName});");
            //componentsDic.Add(inputPlaneID.ToString(), inputPlaneID);
 
        }*/
        //TODO:生成封装组件事件的方法
        pasteContent.Append("\n");
        pasteContent.Append("\t}");
        pasteContent.Append("\n\t#endregion\n");
        string scriptPath;
        if (selectObj.GetComponent<BaseControllerUI>() != null)
        {
            scriptPath = GetScriptPath(selectObj.GetComponent<BaseControllerUI>().GetType());
        }
        else if (selectObj.GetComponent<SubViewBase>() != null)
        {
            scriptPath = GetScriptPath(selectObj.GetComponent<SubViewBase>().GetType());
        }
        else if (selectObj.GetComponent<ContainerSlot>() != null)
        {
            scriptPath = GetScriptPath(selectObj.GetComponent<ContainerSlot>().GetType());
        }
        else if (selectObj.GetComponent<Popup>() != null)
        {
            scriptPath = GetScriptPath(selectObj.GetComponent<Popup>().GetType());
        }
        else if (selectObj.GetComponent<ContainerInfo>() != null)
        {
            scriptPath = GetScriptPath(selectObj.GetComponent<ContainerInfo>().GetType());
        }
        else
        {
            Debug.LogError("当前选中物体没有对应的脚本");
            return;
        }
        // 读取脚本文件内容
        string scriptContent = File.ReadAllText(scriptPath);
        Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled);
        var match = regex.Match(scriptContent);
        scriptContent = scriptContent.Replace(match.ToString(), pasteContent.ToString());
        File.WriteAllText(scriptPath, scriptContent);
        AssetDatabase.Refresh();
        Debug.Log("生成成功");
    }

    /// <summary>
    /// 根据脚本名字获取脚本的全路径
    /// </summary>
    /// <param name="scriptType"></param>
    /// <returns></returns>
    private static string GetScriptPath(Type scriptType)
    {
        string _scriptName = scriptType.Name;
        string[] guidArray = UnityEditor.AssetDatabase.FindAssets(_scriptName);
        foreach (string guid in guidArray)
        {
            string scriptFullPath = AssetDatabase.GUIDToAssetPath(guid);
            if (scriptFullPath.EndsWith(_scriptName + ".cs"))
            {
                return scriptFullPath;
            }
        }
        return null;
    }
    private static Dictionary<Type, string> GenerateBtuDic()//寻找里面有没有我定义的类型的键(key)匹配,然后返回我对应的类型?
    {
        Dictionary<Type, string> dic = new Dictionary<Type, string>();
        dic.Add(typeof(Button), "btn");
        dic.Add(typeof(Scrollbar), "scrB");
        dic.Add(typeof(Image), "img");
        dic.Add(typeof(UICustomText), "text");
        dic.Add(typeof(LayoutGroup), "layoutGroup");
        dic.Add(typeof(SubViewBase), "subView");
        return dic;
    }


}