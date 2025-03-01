using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class GameGlobalFunc
{
    /// <summary>
    /// 强制刷新物体(包括子物体, 按照层级逆序刷新)的 contentSizeFitter
    /// </summary>
    /// <param name="obj"></param>
    public static void ForceRefreshContentSizeFitter(GameObject obj)
    {
        var fitters = obj.GetComponentsInChildren<ContentSizeFitter>();
        for (int i = fitters.Length - 1; i >= 0; i--)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fitters[i].RectTransform());
        }
    }
}
