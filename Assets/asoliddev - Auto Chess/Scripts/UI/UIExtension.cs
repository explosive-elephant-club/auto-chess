using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class UIExtension
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>()?? obj.AddComponent<T>();
    }
    
    public static RectTransform RectTransform(this Component cp){
        return cp.transform as RectTransform;
    }
}
