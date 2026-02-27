/// <summary>
/// 全局技能事件中心（单例）
/// 用于跨角色的技能事件监听，如战斗统计、成就系统等
/// </summary>
public class GlobalSkillEventCenter
{
    private static GlobalSkillEventCenter _instance;
    public static GlobalSkillEventCenter Instance => _instance ??= new GlobalSkillEventCenter();

    private readonly EventCenter _eventCenter = new();

    /// <summary>
    /// 添加技能事件监听器
    /// </summary>
    public void AddListener(SkillEventType eventType, CallBack<SkillEventArgs> callback)
    {
        _eventCenter.AddListener(eventType.ToString(), callback);
    }

    /// <summary>
    /// 移除技能事件监听器
    /// </summary>
    public void RemoveListener(SkillEventType eventType, CallBack<SkillEventArgs> callback)
    {
        _eventCenter.RemoveListener(eventType.ToString(), callback);
    }

    /// <summary>
    /// 广播技能事件
    /// </summary>
    public void Broadcast(SkillEventType eventType, SkillEventArgs args)
    {
        _eventCenter.Broadcast(eventType.ToString(), args);
    }

    /// <summary>
    /// 清除全局事件中心
    /// </summary>
    public static void Clear()
    {
        _instance = null;
    }
}
