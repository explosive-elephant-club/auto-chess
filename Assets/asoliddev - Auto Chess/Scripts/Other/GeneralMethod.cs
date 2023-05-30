using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}