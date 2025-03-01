using System.IO;
using UnityEditor;
using UnityEngine;

public class CustomToolsFunc
{
    [MenuItem("GameObject/资源相关/批量替换Text为UICustomText(不丢引用继承原始参数)", priority = 1)]
    private static void ReplaceTextInPrefabsTextToUICustomText()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue; 

            string content = File.ReadAllText(path);
            string modifiedContent = content.Replace("5f7201a12d95ffc409449d95f23cf332", "067225d7ffa5d32458039dd8be631b78"); // 替换文本

            File.WriteAllText(path, modifiedContent);
            Debug.Log($"Replaced text in {path}");
        }
        AssetDatabase.Refresh(); 
    }
}
