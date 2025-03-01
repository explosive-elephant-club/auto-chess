using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ResourceManager
{
    private const string PREFAB_PATH = "Prefab/";
    private const string SPRITES_PATH = "Sprite/";
    private const string MATERIALS_PATH = "Materials/";
    private const string FONT_PATH = "Fonts/";

    public static T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(Sprite))
        {
            path = SPRITES_PATH + path;
        }
        else if (typeof(T) == typeof(Material))
        {
            path = MATERIALS_PATH + path;
        }else if (typeof(T) == typeof(Font))
        {
            path = FONT_PATH + path;
        }
        return Resources.Load<T>(path);
    }
    
    public static GameObject LoadGameObjectResource(string path, Transform parent = null)
    {
        var obj = Resources.Load<GameObject>(PREFAB_PATH + path);
        if (obj == null)
        {
            Debug.LogError("GameObject not found at path: " + path);
            return null;
        }
        obj = Object.Instantiate(obj, parent);
        return obj;
    }
}
