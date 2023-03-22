using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCenter
{

    private static Dictionary<String, Delegate> m_EventTable = new Dictionary<String, Delegate>();

    private static void OnListenerAdding(String eventKey, Delegate callBack)
    {
        if (!m_EventTable.ContainsKey(eventKey))
        {
            m_EventTable.Add(eventKey, null);
        }
        Delegate d = m_EventTable[eventKey];
        if (d != null && d.GetType() != callBack.GetType())
        {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}", eventKey, d.GetType(), callBack.GetType()));
        }
    }
    private static void OnListenerRemoving(String eventKey, Delegate callBack)
    {
        if (m_EventTable.ContainsKey(eventKey))
        {
            Delegate d = m_EventTable[eventKey];
            if (d == null)
            {
                throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventKey));
            }
            else if (d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同类型的委托，当前委托类型为{1}，要移除的委托类型为{2}", eventKey, d.GetType(), callBack.GetType()));
            }
        }
        else
        {
            throw new Exception(string.Format("移除监听错误：没有事件码{0}", eventKey));
        }
    }
    private static void OnListenerRemoved(String eventKey)
    {
        if (m_EventTable[eventKey] == null)
        {
            m_EventTable.Remove(eventKey);
        }
    }
    //no parameters
    public static void AddListener(String eventKey, CallBack callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack)m_EventTable[eventKey] + callBack;
    }
    //Single parameters
    public static void AddListener<T>(String eventKey, CallBack<T> callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T>)m_EventTable[eventKey] + callBack;
    }
    //two parameters
    public static void AddListener<T, X>(String eventKey, CallBack<T, X> callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X>)m_EventTable[eventKey] + callBack;
    }
    //three parameters
    public static void AddListener<T, X, Y>(String eventKey, CallBack<T, X, Y> callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y>)m_EventTable[eventKey] + callBack;
    }
    //four parameters
    public static void AddListener<T, X, Y, Z>(String eventKey, CallBack<T, X, Y, Z> callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y, Z>)m_EventTable[eventKey] + callBack;
    }
    //five parameters
    public static void AddListener<T, X, Y, Z, W>(String eventKey, CallBack<T, X, Y, Z, W> callBack)
    {
        OnListenerAdding(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y, Z, W>)m_EventTable[eventKey] + callBack;
    }

    //no parameters
    public static void RemoveListener(String eventKey, CallBack callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }
    //single parameters
    public static void RemoveListener<T>(String eventKey, CallBack<T> callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T>)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }
    //two parameters
    public static void RemoveListener<T, X>(String eventKey, CallBack<T, X> callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X>)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }
    //three parameters
    public static void RemoveListener<T, X, Y>(String eventKey, CallBack<T, X, Y> callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y>)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }
    //four parameters
    public static void RemoveListener<T, X, Y, Z>(String eventKey, CallBack<T, X, Y, Z> callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y, Z>)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }
    //five parameters
    public static void RemoveListener<T, X, Y, Z, W>(String eventKey, CallBack<T, X, Y, Z, W> callBack)
    {
        OnListenerRemoving(eventKey, callBack);
        m_EventTable[eventKey] = (CallBack<T, X, Y, Z, W>)m_EventTable[eventKey] - callBack;
        OnListenerRemoved(eventKey);
    }


    //no parameters
    public static void Broadcast(String eventKey)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack callBack = d as CallBack;
            if (callBack != null)
            {
                callBack();
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
    //single parameters
    public static void Broadcast<T>(String eventKey, T arg)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack<T> callBack = d as CallBack<T>;
            if (callBack != null)
            {
                callBack(arg);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
    //two parameters
    public static void Broadcast<T, X>(String eventKey, T arg1, X arg2)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack<T, X> callBack = d as CallBack<T, X>;
            if (callBack != null)
            {
                callBack(arg1, arg2);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
    //three parameters
    public static void Broadcast<T, X, Y>(String eventKey, T arg1, X arg2, Y arg3)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack<T, X, Y> callBack = d as CallBack<T, X, Y>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
    //four parameters
    public static void Broadcast<T, X, Y, Z>(String eventKey, T arg1, X arg2, Y arg3, Z arg4)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack<T, X, Y, Z> callBack = d as CallBack<T, X, Y, Z>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3, arg4);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
    //five parameters
    public static void Broadcast<T, X, Y, Z, W>(String eventKey, T arg1, X arg2, Y arg3, Z arg4, W arg5)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventKey, out d))
        {
            CallBack<T, X, Y, Z, W> callBack = d as CallBack<T, X, Y, Z, W>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventKey));
            }
        }
    }
}