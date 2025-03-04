using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace General
{
    public static class GeneralMethod
    {
        public static bool FindMethodByName(object _class, string methodName)
        {
            MethodInfo method = _class.GetType().GetMethod(methodName);
            return method != null;
        }

        public static void ExecuteMethodByName(object _class, string methodName, object[] args = null)
        {
            MethodInfo method = _class.GetType().GetMethod(methodName);
            if (method != null)
            {
                args = args == null ? new object[0] : args;
                method.Invoke(_class, args);
            }
            else
            {
                Debug.LogWarning("Method not found: " + methodName + " in " + _class.GetType().Name);
            }
        }

        public static object GetValueByName(object _class, string name)
        {
            FieldInfo f = _class.GetType().GetField(name);
            if (f != null)
            {
                return f.GetValue(_class);
            }
            return f;
        }

        /// <summary>
        /// 向上遍历强制刷新所有的contentSizeFitter
        /// </summary>
        /// <param name="target">起始节点</param>
        public static void ForceRefreshContentSizeFitterUpwards(Transform target)
        {
            Transform parent = target;
            while (parent != null)
            {
                if (parent.GetComponent<ContentSizeFitter>() != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
                }
                if (parent.parent != null)
                {
                    parent = parent.parent;
                }
                else
                {
                    parent = null;
                }
            }
        }

        /// <summary>
        /// 强制刷新子物体和自身所有的contentSizeFitter
        /// </summary>
        /// <param name="target">根节点</param>
        public static void ForceRefreshAllContentSizeFitter(Transform target)
        {
            Debug.Log("强制刷新子物体");
            var fitters = target.GetComponentsInChildren<ContentSizeFitter>();
            for (int i = fitters.Length - 1; i >= 0; i--)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitters[i].RectTransform());
            }
        }
    }
}