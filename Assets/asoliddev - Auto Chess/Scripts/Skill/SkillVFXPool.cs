using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 技能VFX对象池
/// 管理技能特效的实例化和回收，减少 Instantiate/Destroy 开销
/// </summary>
public class SkillVFXPool
{
    private static SkillVFXPool _instance;
    public static SkillVFXPool Instance => _instance ??= new SkillVFXPool();

    /// <summary>
    /// VFX类型
    /// </summary>
    public enum VFXType
    {
        Effect,
        Emit,
        Hit
    }

    private readonly SkillVFXLoader _vfxLoader = SkillVFXLoader.Instance;
    
    // skillId -> (vfxType -> pool)
    private readonly Dictionary<int, Dictionary<VFXType, Queue<GameObject>>> _pools = new();
    
    // 预制体原始状态缓存（按预制体存储，所有实例共享）
    // key: skillId * 100 + vfxType
    private readonly Dictionary<int, PrefabOriginalState> _prefabStates = new();

    // 每种类型的最大池大小
    private const int MAX_POOL_SIZE_PER_TYPE = 10;
    
    private static int GetPrefabKey(int skillId, VFXType type) => skillId * 100 + (int)type;

    /// <summary>
    /// 从池中获取特效实例
    /// </summary>
    public GameObject Get(int skillId, VFXType type, Vector3 pos, Quaternion rot)
    {
        if (_pools.TryGetValue(skillId, out var typeDict)
            && typeDict.TryGetValue(type, out var queue)
            && queue.Count > 0)
        {
            var obj = queue.Dequeue();
            if (obj != null)
            {
                // 重置到原始状态（包括 Transform、子对象、Collider 等）
                ResetToOriginalState(obj, skillId, type, pos, rot);
                
                // 取消任何待处理的归还计时器
                CancelPendingReturn(obj);
                
                // 先激活对象
                obj.SetActive(true);
                
                // 重置粒子系统
                ResetParticleSystems(obj);
                
                // 调用 IPoolable.OnSpawn（如果实现了接口）
                NotifyOnSpawn(obj);
                
                return obj;
            }
        }

        // 池中没有，新建并缓存原始 Transform
        return InstantiateNew(skillId, type, pos, rot);
    }

    /// <summary>
    /// 将特效实例归还到池中
    /// </summary>
    public void Return(int skillId, VFXType type, GameObject obj)
    {
        if (obj == null) return;

        // 调用 IPoolable.OnDespawn（如果实现了接口）
        NotifyOnDespawn(obj);
        
        // 停止所有 DOTween 动画
        obj.transform.DOKill();
        
        // 停止所有粒子系统
        StopParticleSystems(obj);
        
        // 禁用对象
        obj.SetActive(false);
        
        var queue = EnsurePool(skillId, type);
        
        if (queue.Count < MAX_POOL_SIZE_PER_TYPE)
        {
            queue.Enqueue(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }

    /// <summary>
    /// 延迟归还特效（用于有持续时间的特效）
    /// </summary>
    public void ReturnDelayed(int skillId, VFXType type, GameObject obj, float delay)
    {
        if (obj == null) return;
        
        var returner = obj.GetComponent<VFXPoolReturner>();
        if (returner == null)
        {
            returner = obj.AddComponent<VFXPoolReturner>();
        }
        returner.Setup(skillId, type, delay);
    }

    /// <summary>
    /// 预热指定技能的特效池
    /// </summary>
    public void PreWarm(int skillId, VFXType type, int count)
    {
        var prefab = GetPrefab(skillId, type);
        if (prefab == null) return;

        var queue = EnsurePool(skillId, type);
        for (int i = 0; i < count && queue.Count < MAX_POOL_SIZE_PER_TYPE; i++)
        {
            var obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空所有池
    /// </summary>
    public void ClearAll()
    {
        foreach (var skillDict in _pools.Values)
        {
            foreach (var queue in skillDict.Values)
            {
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    if (obj != null)
                    {
                        Object.Destroy(obj);
                    }
                }
            }
        }
        _pools.Clear();
        _prefabStates.Clear();
    }

    /// <summary>
    /// 清空指定技能的池
    /// </summary>
    public void Clear(int skillId)
    {
        if (_pools.TryGetValue(skillId, out var typeDict))
        {
            foreach (var queue in typeDict.Values)
            {
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    if (obj != null)
                    {
                        Object.Destroy(obj);
                    }
                }
            }
            _pools.Remove(skillId);
        }
    }

    private GameObject InstantiateNew(int skillId, VFXType type, Vector3 pos, Quaternion rot)
    {
        var prefab = GetPrefab(skillId, type);
        if (prefab == null) return null;

        // 缓存预制体原始状态（只缓存一次，所有实例共享）
        var prefabKey = GetPrefabKey(skillId, type);
        if (!_prefabStates.ContainsKey(prefabKey))
        {
            _prefabStates[prefabKey] = PrefabOriginalState.ExtractFrom(prefab);
        }

        var obj = Object.Instantiate(prefab, pos, rot);
        return obj;
    }

    /// <summary>
    /// 重置 Transform 和组件到原始状态
    /// </summary>
    private void ResetToOriginalState(GameObject obj, int skillId, VFXType type, Vector3 pos, Quaternion rot)
    {
        // 停止所有 DOTween 动画
        obj.transform.DOKill();
        
        // 重置位置和旋转
        obj.transform.SetPositionAndRotation(pos, rot);
        
        // 使用预制体级别的缓存恢复原始状态
        var prefabKey = GetPrefabKey(skillId, type);
        if (_prefabStates.TryGetValue(prefabKey, out var prefabState))
        {
            prefabState.ApplyTo(obj);
        }
    }

    /// <summary>
    /// 取消待处理的归还计时器
    /// </summary>
    private void CancelPendingReturn(GameObject obj)
    {
        var returner = obj.GetComponent<VFXPoolReturner>();
        if (returner != null)
        {
            returner.Cancel();
        }
    }

    /// <summary>
    /// 通知所有 IPoolable 组件对象已从池中取出
    /// </summary>
    private void NotifyOnSpawn(GameObject obj)
    {
        var poolables = obj.GetComponentsInChildren<IPoolable>(true);
        foreach (var poolable in poolables)
        {
            poolable.OnSpawn();
        }
    }

    /// <summary>
    /// 通知所有 IPoolable 组件对象即将归还到池
    /// </summary>
    private void NotifyOnDespawn(GameObject obj)
    {
        var poolables = obj.GetComponentsInChildren<IPoolable>(true);
        foreach (var poolable in poolables)
        {
            poolable.OnDespawn();
        }
    }

    /// <summary>
    /// 停止所有粒子系统和拖尾
    /// </summary>
    private void StopParticleSystems(GameObject obj)
    {
        var particleSystems = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        var trailRenderers = obj.GetComponentsInChildren<TrailRenderer>(true);
        foreach (var trail in trailRenderers)
        {
            trail.Clear();
        }
    }

    private GameObject GetPrefab(int skillId, VFXType type)
    {
        return type switch
        {
            VFXType.Effect => _vfxLoader.GetEffectPrefab(skillId),
            VFXType.Emit => _vfxLoader.GetEmitPrefab(skillId),
            VFXType.Hit => _vfxLoader.GetHitPrefab(skillId),
            _ => null
        };
    }

    private Queue<GameObject> EnsurePool(int skillId, VFXType type)
    {
        if (!_pools.TryGetValue(skillId, out var typeDict))
        {
            typeDict = new Dictionary<VFXType, Queue<GameObject>>();
            _pools[skillId] = typeDict;
        }

        if (!typeDict.TryGetValue(type, out var queue))
        {
            queue = new Queue<GameObject>();
            typeDict[type] = queue;
        }

        return queue;
    }

    private void ResetParticleSystems(GameObject obj)
    {
        // 重置粒子系统
        var particleSystems = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            // 完全重置粒子系统
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
            
            // 重置模拟时间
            ps.Simulate(0, true, true, false);
            
            // 重新播放
            ps.Play(true);
        }
        
        // 重置拖尾渲染器
        var trailRenderers = obj.GetComponentsInChildren<TrailRenderer>(true);
        foreach (var trail in trailRenderers)
        {
            trail.Clear();
        }
    }

    /// <summary>
    /// 重置单例（用于场景切换等情况）
    /// </summary>
    public static void Reset()
    {
        _instance?.ClearAll();
        _instance = null;
    }
}

/// <summary>
/// 可池化对象接口
/// 实现此接口的对象可以自定义从池中取出和归还时的行为
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 从池中取出时调用
    /// </summary>
    void OnSpawn();
    
    /// <summary>
    /// 归还到池中时调用
    /// </summary>
    void OnDespawn();
}

/// <summary>
/// 预制体原始状态数据（按预制体存储，所有实例共享）
/// </summary>
public class PrefabOriginalState
{
    public struct TransformState
    {
        public int SiblingIndex;  // 用于匹配子对象
        public string Name;       // 用于匹配子对象
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    public struct ColliderState
    {
        public int SiblingIndex;
        public string Name;
        public bool Enabled;
    }

    public Vector3 RootLocalScale;
    public TransformState[] ChildStates;
    public ColliderState[] ColliderStates;

    /// <summary>
    /// 从预制体提取原始状态
    /// </summary>
    public static PrefabOriginalState ExtractFrom(GameObject prefab)
    {
        var state = new PrefabOriginalState
        {
            RootLocalScale = prefab.transform.localScale
        };

        // 提取子 Transform 状态
        var childTransforms = prefab.GetComponentsInChildren<Transform>(true);
        var childList = new List<TransformState>();
        foreach (var t in childTransforms)
        {
            if (t == prefab.transform) continue;
            childList.Add(new TransformState
            {
                SiblingIndex = t.GetSiblingIndex(),
                Name = t.name,
                LocalPosition = t.localPosition,
                LocalRotation = t.localRotation,
                LocalScale = t.localScale
            });
        }
        state.ChildStates = childList.ToArray();

        // 提取 Collider 状态
        var colliders = prefab.GetComponentsInChildren<Collider>(true);
        var colliderList = new List<ColliderState>();
        foreach (var c in colliders)
        {
            colliderList.Add(new ColliderState
            {
                SiblingIndex = c.transform.GetSiblingIndex(),
                Name = c.name,
                Enabled = c.enabled
            });
        }
        state.ColliderStates = colliderList.ToArray();

        return state;
    }

    /// <summary>
    /// 将原始状态应用到实例
    /// </summary>
    public void ApplyTo(GameObject instance)
    {
        instance.transform.localScale = RootLocalScale;

        // 恢复子 Transform（通过名称匹配）
        if (ChildStates != null)
        {
            var childTransforms = instance.GetComponentsInChildren<Transform>(true);
            var nameToTransform = new Dictionary<string, Transform>();
            foreach (var t in childTransforms)
            {
                if (t != instance.transform && !nameToTransform.ContainsKey(t.name))
                {
                    nameToTransform[t.name] = t;
                }
            }

            foreach (var state in ChildStates)
            {
                if (nameToTransform.TryGetValue(state.Name, out var t))
                {
                    t.localPosition = state.LocalPosition;
                    t.localRotation = state.LocalRotation;
                    t.localScale = state.LocalScale;
                }
            }
        }

        // 恢复 Collider 状态（通过名称匹配）
        if (ColliderStates != null)
        {
            var colliders = instance.GetComponentsInChildren<Collider>(true);
            var nameToCollider = new Dictionary<string, Collider>();
            foreach (var c in colliders)
            {
                if (!nameToCollider.ContainsKey(c.name))
                {
                    nameToCollider[c.name] = c;
                }
            }

            foreach (var state in ColliderStates)
            {
                if (nameToCollider.TryGetValue(state.Name, out var c))
                {
                    c.enabled = state.Enabled;
                }
            }
        }
    }
}

/// <summary>
/// VFX池归还组件
/// 用于延迟归还特效到池中
/// </summary>
public class VFXPoolReturner : MonoBehaviour
{
    private int _skillId;
    private SkillVFXPool.VFXType _vfxType;
    private float _delay;
    private float _timer;
    private bool _isSetup;
    private bool _isReturned;

    public void Setup(int skillId, SkillVFXPool.VFXType vfxType, float delay)
    {
        _skillId = skillId;
        _vfxType = vfxType;
        _delay = delay;
        _timer = 0f;
        _isSetup = true;
        _isReturned = false;
    }

    /// <summary>
    /// 取消归还（当对象被重新使用时调用）
    /// </summary>
    public void Cancel()
    {
        _isSetup = false;
        _isReturned = false;
        _timer = 0f;
    }

    private void OnEnable()
    {
        // 对象被重新激活时，如果正在倒计时则取消
        if (_isSetup && !_isReturned)
        {
            Cancel();
        }
    }

    private void Update()
    {
        if (!_isSetup || _isReturned) return;

        _timer += Time.deltaTime;
        if (_timer >= _delay)
        {
            _isSetup = false;
            _isReturned = true;
            SkillVFXPool.Instance.Return(_skillId, _vfxType, gameObject);
        }
    }

    private void OnDisable()
    {
        // 对象被禁用时重置状态
        _isSetup = false;
        _timer = 0f;
    }
}
