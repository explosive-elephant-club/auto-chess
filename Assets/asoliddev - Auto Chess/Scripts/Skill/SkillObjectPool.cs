using System;
using System.Collections.Generic;

/// <summary>
/// 通用对象池
/// 用于减少频繁创建和销毁对象带来的GC开销
/// </summary>
/// <typeparam name="T">池化对象类型</typeparam>
public class SkillObjectPool<T> where T : class, new()
{
    private readonly Stack<T> _pool = new();
    private readonly Action<T> _resetAction;
    private readonly int _maxSize;

    /// <summary>
    /// 当前池中对象数量
    /// </summary>
    public int Count => _pool.Count;

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="resetAction">对象归还时的重置回调</param>
    /// <param name="preWarm">预热数量</param>
    /// <param name="maxSize">池最大容量（0表示无限制）</param>
    public SkillObjectPool(Action<T> resetAction = null, int preWarm = 0, int maxSize = 0)
    {
        _resetAction = resetAction;
        _maxSize = maxSize;
        
        for (int i = 0; i < preWarm; i++)
        {
            _pool.Push(new T());
        }
    }

    /// <summary>
    /// 从池中获取对象
    /// </summary>
    public T Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : new T();
    }

    /// <summary>
    /// 将对象归还到池中
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null) return;
        
        if (_maxSize > 0 && _pool.Count >= _maxSize)
            return;
        
        _resetAction?.Invoke(obj);
        _pool.Push(obj);
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        _pool.Clear();
    }
}
