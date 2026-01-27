# 技能系统技术文档

> 版本：1.0  
> 最后更新：2026-01-27

---

## 目录

1. [系统概述](#1-系统概述)
2. [架构设计](#2-架构设计)
3. [核心类详解](#3-核心类详解)
4. [技能执行流程](#4-技能执行流程)
5. [弹道与攻击类型](#5-弹道与攻击类型)
6. [目标选择系统](#6-目标选择系统)
7. [伤害与命中处理](#7-伤害与命中处理)
8. [VFX资源管理](#8-vfx资源管理)
9. [配置数据结构](#9-配置数据结构)
10. [扩展指南](#10-扩展指南)

---

## 1. 系统概述

技能系统采用 **状态机驱动** 的设计模式，将技能的 **数据**、**状态**、**执行逻辑** 完全解耦，支持：

- **10种弹道类型**：波形、范围、环绕、火箭、激光、投掷物、子弹等
- **灵活的目标选择**：支持自身/队友/敌人，多种选择策略（最近、最远、血量最多/最少等）
- **多种伤害逻辑**：碰撞伤害、持续伤害、延迟范围伤害、护盾等
- **技能链机制**：多技能顺序释放与CD管理
- **脱手技能**：释放后自动执行，不阻塞后续技能

### 设计原则

| 原则 | 说明 |
|------|------|
| 数据驱动 | 技能行为由 Excel 配置 (`SkillData`) 决定，无需修改代码 |
| 状态机模式 | 使用 `SkillPhase` 枚举管理技能生命周期 |
| 单一职责 | 目标查找、效果创建、命中处理分别由独立类负责 |
| 接口抽象 | `ISkillState` 和 `ISkillExecutionContext` 提供扩展点 |

---

## 2. 架构设计

### 2.1 分层架构图

```Mermaid
graph TB
    subgraph "第1层：控制层 Controller"
        sc(SkillController<br/>角色技能管理器)
        sep(SkillExeProcess<br/>技能执行器)
        sc --> sep
    end
    
    subgraph "第2层：状态机层 StateMachine"
        ssm(SkillStateMachine<br/>状态机驱动<br/>实现ISkillExecutionContext)
        sr(SkillRuntime<br/>运行时数据容器)
        ssm --> sr
    end
    
    subgraph "第3层：技能定义与功能层"
        subgraph "效果层 Effect"
            ef(SkillEffectFactory)
            sh(SkillHitHandler)
            si(SkillInstance)
        end
        
        subgraph "目标层 Target"
            stf(SkillTargetFinder)
            sts(SkillTargetsSelector)
        end
        
        subgraph "技能定义层 Skill"
            iss(ISkillState)
            sb(SkillBase)
            scx(SkillContext)
        end
    end
    
    %% 层间连接
    sep --> ssm
    ssm -.-> |驱动流程| ef
    ssm -.-> |查找目标| stf
    ssm -.-> |获取定义| sb
    
    %% 层内依赖
    ef -.-> |调用| sh
    stf -.-> |使用| sts
    sb -.-> |管理| scx
    
    style sc fill:#e1f5fe
    style sep fill:#e1f5fe
    style ssm fill:#f3e5f5
    style sr fill:#f3e5f5
    style ef fill:#e8f5e8
    style sh fill:#e8f5e8
    style si fill:#e8f5e8
    style stf fill:#fff3e0
    style sts fill:#fff3e0
    style iss fill:#ffebee
    style sb fill:#ffebee
    style scx fill:#ffebee
```


### 2.2 核心类关系

```
ChampionController
       │
       ├── skillController: SkillController
       │         │
       │         ├── skillList: List<ISkillState>
       │         ├── activedSkillList: List<ISkillState>
       │         └── _skillExeProcess: SkillExeProcess
       │                    │
       │                    └── SingleSkillExeProcess
       │                              │
       │                              ├── _runtime: SkillRuntime
       │                              └── _stateMachine: SkillStateMachine
       │                                        │
       │                                        ├── _targetFinder: SkillTargetFinder
       │                                        ├── _effectFactory: SkillEffectFactory
       │                                        ├── _hitHandler: SkillHitHandler
       │                                        └── _vfxLoader: SkillVFXLoader
       │
       └── championCombatController: ChampionCombatController
                    │
                    └── TakeDamage() ← 被 SkillHitHandler 调用
```

---

## 3. 核心类详解

### 3.1 文件清单

| 文件 | 类名 | 职责 |
|------|------|------|
| `SkillDefine.cs` | `ISkillState`, `SkillPhase`, `SkillContext`, `SkillHelper`, `SkillFactory` | 接口定义、枚举、上下文、工具类、工厂 |
| `SkillBase.cs` | `SkillBase` | 技能基类，实现 `ISkillState` |
| `SkillController.cs` | `SkillController` | 角色的技能管理器，管理技能链 |
| `SkillStateMachine.cs` | `SkillStateMachine` | 技能状态机，驱动技能执行，实现 `ISkillExecutionContext` |
| `SkillExeProcess.cs` | `SkillExeProcess`, `SingleSkillExeProcess` | 技能执行器，解耦数据与行为 |
| `SkillRuntime.cs` | `SkillRuntime` | 技能运行时数据容器 |
| `SkillTargetFinder.cs` | `SkillTargetFinder`, `TargetResult` | 统一目标查找器 |
| `SkillTargetsSelector.cs` | `SkillTargetsSelector`, `SelectorResult` | 底层目标选择器 |
| `SkillInstance.cs` | `SkillInstance` | 技能投射物实例（MonoBehaviour） |
| `SkillEffectFactory.cs` | `SkillEffectFactory`, `EffectCreateContext` | 效果/投射物工厂 |
| `SkillHitHandler.cs` | `SkillHitHandler`, `HitContext`, `DirectEffectContext` | 命中处理（伤害、Buff、特效） |
| `SkillDamageMethods.cs` | `SkillDamageMethods` | 伤害逻辑扩展方法 |
| `SkillMoveMethods.cs` | `SkillMoveMethods` | 移动逻辑扩展方法 |
| `SkillMoveContext.cs` | `SkillMoveContext` | 移动上下文（语义化参数） |
| `SkillVFXLoader.cs` | `SkillVFXLoader`, `VFXBundle` | VFX资源预加载和缓存 |
| `SkillConstants.cs` | `SkillConstants` | 常量定义 |
| `ISkillExecutionContext.cs` | `ISkillExecutionContext` | 技能执行上下文接口 |

---

### 3.2 ISkillState 接口

```csharp
public interface ISkillState
{
    void ResetSkillContext(bool isResetCount = false);  // 重置技能上下文
    SkillData GetSkillCfg();                            // 获取技能配置
    float GetSkillCastDelay();                          // 获取施法延迟
    float GetSkillChargingDelay();                      // 获取充能延迟
    bool CheckIsSellSkill();                            // 是否为脱手技能
    bool IsPrepared();                                  // 技能是否就绪
    bool HaveTargetInRange();                           // 是否有目标在范围内
    SkillContext GetContext();                          // 获取技能上下文
    void TickCd();                                      // CD计时
    void StartSkillChainCdCutDown(float cd);            // 开始技能链CD
    ConstructorBase GetConstructor();                   // 获取所属部件
    ChampionController FindAvailableTarget();           // 查找可用目标
    ChampionController GetOwner();                      // 获取拥有者
    void StartCastCdCutDown();                          // 开始施法CD
    bool IsStartCd();                                   // 是否在CD中
    List<ChampionController> GetTargetList();           // 获取目标列表
}
```

---

### 3.3 SkillPhase 枚举

技能执行阶段的统一枚举：

```csharp
public enum SkillPhase
{
    Idle,           // 空闲/未激活
    WaitingCD,      // 等待CD
    FindingTarget,  // 寻找目标
    Casting,        // 施法动作中
    Executing,      // 效果执行中（脱手后）
    Finished,       // 本次释放完成
    Failed          // 释放失败
}
```

---

### 3.4 SkillContext 类

技能全局数据，用于修改技能行为和判断技能是否可释放：

```csharp
public class SkillContext
{
    public SkillState State;                            // 技能UI状态 (Disable/Activied)
    public float IntervalTime;                          // 技能生效间隔
    public int DamageProportion;                        // 基础伤害比例
    public float Range;                                 // 效果范围
    public int CountRemain;                             // 剩余可用次数
    public bool IsSell;                                 // 技能是否脱手
    public SkillTargetType SkillTargetType;             // 效果目标类型
    public SkillRangeSelectorType SkillRangeSelectorType;    // 效果目标选中方式
    public SkillTargetSelectorType SkillTargetSelectorType;  // 效果目标选择器类型
    public float CurAllCutDown;                         // 当前总CD
    public float CdCutDown;                             // CD倒计时
}
```

---

### 3.5 SkillRuntime 类

技能运行时数据，包含技能执行过程中需要的所有运行时信息：

**基础引用：**
- `SkillState`: ISkillState 接口
- `SkillData`: 技能配置数据
- `Owner`: ChampionController 拥有者
- `Constructor`: ConstructorBase 所属部件
- `Context`: SkillContext 技能上下文

**执行状态：**
- `CurrentPhase`: 当前技能阶段
- `CurrentDuration`: 当前持续时间
- `CurrentEffectCount`: 当前生效次数
- `CurrentIntervalTime`: 当前生效间隔计时
- `CurrentCastPointIndex`: 当前发射点索引
- `HasStartedCD`: 技能是否已进入CD
- `CanFinish`: 技能是否可以结束
- `PendingCounter`: 未完成计数器（用于多弹道技能）

**伤害相关：**
- `TotalHitCount`: 总命中次数限制
- `CurrentHitCount`: 当前命中次数

**技能实例：**
- `EffectInstances`: List<SkillInstance> 已生成的技能实例列表

**技能逻辑：**
- `AttackType`: 攻击类型
- `LogicData`: 逻辑数据 (MoveLogic, DamageLogic, 等)
- `IsDamageDestroySkill`: 是否为伤害后消失类技能
- `NeedsContinuousCasting`: 是否需要持续施法

---

### 3.6 ISkillExecutionContext 接口

供 `SkillInstance` 和 `SkillDamageMethods` 访问技能执行状态：

```csharp
public interface ISkillExecutionContext
{
    SkillHelper.SkillLogicData LogicData { get; }       // 技能逻辑数据
    SkillHelper.SkillAttackType AttackType { get; }     // 攻击类型
    SkillTargetType SkillTargetType { get; }            // 技能目标类型
    ChampionTeam Team { get; }                          // 所属队伍
    
    float GetMoveSpeed();                               // 获取移动速度
    Transform ReGetTarget();                            // 重新获取目标
    void SetCanFinish(bool canFinish);                  // 设置技能是否可以结束
    void SkillHitEffect(Collider collider, ChampionController target, bool onlyEffect = false);  // 处理技能命中效果
}
```

---

## 4. 技能执行流程

### 4.1 状态转换图

```Mermaid
stateDiagram-v2
    [*] --> Idle
    
    Idle --> FindingTarget: 需要释放技能<br/>+ 有目标在范围内
    FindingTarget --> Casting: 成功找到目标
    FindingTarget --> Failed: 未找到目标<br/>或超过射程
    Failed --> WaitingCD: CD开始
    
    Casting --> Executing: 效果脱手<br/>或持续施法技能
    Casting --> WaitingCD: 直接效果<br/>(瞬发技能)
    
    Executing --> WaitingCD: 持续时间结束<br/>或命中次数达标
    Executing --> Executing: 持续命中处理
    
    WaitingCD --> Idle: CD结束<br/>技能重置
    
    %% 状态详细行为
    note right of Idle
        技能可用且CD结束
        检查目标是否在射程内
    end note
    
    note right of FindingTarget
        转向目标
        检查技能是否可用(法力、次数)
        开始施法Cast()
    end note
    
    note right of Casting
        播放施法动画
        扣除法力值
        ↓生成效果↓
        直接效果：ExecuteDirectEffect()
        投射物：CreateProjectileEffect()
    end note
    
    note right of Executing
        更新所有技能实例
        处理命中逻辑
        检查是否应结束(ShouldFinish)
    end note
    
    note right of Failed
        目标丢失
        法力不足
        被中断
    end note
    
    note right of WaitingCD
        技能链CD计时
        等待下次释放机会
    end note
```


### 4.2 详细执行流程

#### 阶段 1: 技能初始化

```
SkillController.AddSkill()
    │
    ├── SkillFactory.Create() 创建技能实例
    ├── skill.ResetSkillContext(true) 初始化上下文
    └── skillList.Add(skill) 加入技能列表
```

#### 阶段 2: 战斗开始

```
SkillController.OnEnterCombat()
    │
    ├── 重置当前技能索引 _curSkillIndex = 0
    ├── 将所有激活技能加入 _usedSkillList
    └── StartSkillChainCd() 开始技能链CD
```

#### 阶段 3: 每帧更新

```
SkillController.Tick()
    │
    ├── _skillExeProcess.ExecuteSkill()
    │         │
    │         └── SingleSkillExeProcess.TickSkill()
    │                   │
    │                   └── SkillStateMachine.Tick()
    │
    └── 根据状态判断是否需要获取下一个技能
```

#### 阶段 4: 状态机处理

```
SkillStateMachine.Tick()
    │
    ├── HandleIdle() ──────▶ 检查CD，转到 FindingTarget
    │
    ├── HandleFindingTarget()
    │       ├── 转向目标 Owner.TurnToTarget()
    │       ├── 检查技能是否可用 IsPrepared()
    │       └── 开始施法 Cast()
    │
    ├── HandleCasting()
    │       ├── UpdateEffectInstances() 更新所有技能实例
    │       ├── OnCastingUpdate() 处理效果生成
    │       │       ├── ExecuteDirectEffect() 直接效果
    │       │       └── CreateProjectileEffect() 投射物效果
    │       └── UpdateDuration() 更新持续时间
    │
    └── HandleExecuting()
            ├── UpdateEffectInstances() 更新所有技能实例
            ├── UpdateDuration() 更新持续时间
            └── ShouldFinish() 检查是否应该结束
```

#### 阶段 5: 施法过程

```
Cast()
    │
    ├── 广播 BuffActiveMode.BeforeCast 事件
    ├── 扣除法力值 Owner.attributesController.curMana -= manaCost
    ├── 更新剩余使用次数 Context.CountRemain--
    ├── 播放施法动画 PlayCastAnim()
    └── 广播 BuffActiveMode.AfterCast 事件
```

#### 阶段 6: 效果执行

**直接效果 (isDirectEffect = true)：**
```
ExecuteDirectEffect()
    │
    ├── 获取目标列表 GetTargetList()
    ├── 生成发射特效 CreateEmitEffect()
    └── 对所有目标造成效果 SkillHitHandler.ApplyDirectEffect()
```

**投射物效果 (isDirectEffect = false)：**
```
CreateProjectileEffect()
    │
    ├── 创建效果上下文 EffectCreateContext
    ├── 创建投射物 SkillEffectFactory.CreateProjectile()
    ├── 初始化实例 skillInstance.Init()
    └── 加入实例列表 EffectInstances.Add()
```

---

## 5. 弹道与攻击类型

### 5.1 SkillAttackType 枚举

```csharp
public enum SkillAttackType
{
    None = 0,                          // 无弹道
    WaveformStaticTrajectory = 1,      // 波形静态弹道
    WaveformDynamicTrajectory = 2,     // 波形动态弹道
    RangeTrajectory = 3,               // 范围弹道
    OrbitAroundTrajectory = 4,         // 轨道环绕弹道
    TrajectoryOfRocket = 5,            // 火箭弹道
    LaserSustainedTrajectory = 6,      // 激光持续弹道
    InstantaneousTrajectoryOfLaser = 7,// 激光瞬间弹道
    TrajectoryOfProjectile = 8,        // 投掷物弹道
    TheBulletRicocheted = 9,           // 子弹散弹弹道
    BulletLinearTrajectory = 10,       // 子弹直线弹道
}
```

### 5.2 逻辑数据映射表

每种攻击类型对应一组逻辑数据：

| 攻击类型 | MoveLogic | DamageLogic | DestroyOnColliderShield | IsCreateInSelf |
|----------|-----------|-------------|-------------------------|----------------|
| WaveformStaticTrajectory | None | Normal | false | false |
| WaveformDynamicTrajectory | MoveAndScale | Normal | true | false |
| RangeTrajectory | FollowSelfAndTurnToTarget | Normal | false | false |
| OrbitAroundTrajectory | FollowSelfAndTurnAround | Normal | false | true |
| TrajectoryOfRocket | UpAndFindToTarget | OnlyCollision | false | false |
| LaserSustainedTrajectory | DurationSweep | Normal | false | false |
| InstantaneousTrajectoryOfLaser | OnlyEffect | OnlyCollision | true | false |
| TrajectoryOfProjectile | ParabolaAndRebound | DurationAfterDamage | false | false |
| TheBulletRicocheted | MoveForward | OnlyCollision | true | false |
| BulletLinearTrajectory | MoveForward | OnlyCollision | true | false |

### 5.3 MoveLogic 移动逻辑

```csharp
public enum MoveLogic
{
    None = 0,                      // 生成后不移动
    MoveAndScale = 1,              // 生成后移动并缩放
    FollowSelfAndTurnToTarget = 2, // 跟随自身并转向目标
    FollowSelfAndTurnAround = 3,   // 跟随自身并环绕运动
    UpAndFindToTarget = 4,         // 向上一段距离后寻找目标运动（火箭弹道）
    DurationSweep = 5,             // 持续横扫前方扇形区域
    OnlyEffect = 6,                // 无弹道，只有特效
    ParabolaAndRebound = 7,        // 抛物线运动，碰到地面后小幅反弹
    MoveForward = 8,               // 向前直线运动
}
```

### 5.4 DamageLogic 伤害逻辑

```csharp
public enum DamageLogic
{
    Normal = 0,              // 一次碰撞触发一次伤害，之后间隔触发伤害
    OnlyCollision = 1,       // 只有碰撞触发伤害
    DurationAfterDamage = 2, // 持续时间结束后触发一次范围伤害
    Shield = 3,              // 护盾类型
}
```

---

## 6. 目标选择系统

### 6.1 三层选择策略

目标选择分为三层，逐层筛选：

```
第一层: SkillTargetType (目标阵营)
         │
         ▼
第二层: SkillTargetSelectorType (目标选择策略)
         │
         ▼
第三层: SkillRangeSelectorType (范围扩展)
```

### 6.2 SkillTargetType

目标阵营类型：

| 枚举值 | 说明 |
|--------|------|
| `Self` | 自身 |
| `Teammate` | 队友 |
| `Enemy` | 敌人 |

### 6.3 SkillTargetSelectorType

具体的目标选择策略：

| 枚举值 | 说明 |
|--------|------|
| `Custom` | 自定义 |
| `Any` | 任一目标 |
| `Nearest` | 最近的目标 |
| `Farthest` | 最远的目标 |
| `HighestDPS` | 伤害最高的目标 |
| `HighestLevel` | 等级最高的目标 |
| `MostHP` | 生命值最多的目标 |
| `LeastHP` | 生命值最少的目标（按百分比计算） |
| `MostTeammatesSurrounded` | 周围友军最多的目标 |
| `MostEnemiesSurrounded` | 周围敌人最多的目标 |

### 6.4 SkillRangeSelectorType

范围扩展方式：

| 枚举值 | 说明 |
|--------|------|
| `Custom` | 自定义 |
| `TeammatesInRange` | 范围内所有友军 |
| `EnemiesInRange` | 范围内所有敌人 |
| `MapHexInRange` | 范围内所有棋格 |

### 6.5 目标查找流程

```
SkillTargetFinder.FindTargets(runtime, isIdleFindTarget)
    │
    ├── 如果 SkillTargetType == Self
    │       └── 直接返回 Owner
    │
    └── 否则
            │
            ├── 1. FindTargetsManagerByType() 获取目标管理器
            │       Player队伍 → ownChampionManager
            │       Enemy队伍 → oponentChampionManager
            │
            ├── 2. FindTargetBySelectorType() 根据策略查找主目标
            │       └── 应用距离限制 + 额外射程加成
            │
            └── 3. FindTargetByRange() 扩展到范围内所有目标
                    └── 按距离排序返回
```

---

## 7. 伤害与命中处理

### 7.1 命中处理流程

```
SkillHitHandler.HandleHit(HitContext)
    │
    ├── 1. InstantiateHitEffect() 生成命中特效
    │
    ├── 2. 如果 OnlyEffect == true，跳过后续步骤
    │
    ├── 3. ApplyBuffs() 施加Buff
    │       └── 遍历 skillData.addBuffs
    │           └── target.buffController.AddBuff()
    │
    └── 4. ApplyDamage() 造成伤害
            └── caster.championCombatController.TakeDamage()
```

### 7.2 碰撞伤害处理

`SkillDamageMethods` 提供扩展方法处理不同的伤害逻辑：

```csharp
instance.SkillDamage(damageLogic, self, hit, colliderType)
    │
    ├── 检查护盾碰撞
    │       └── 如果 DestroyOnColliderShield && 碰到敌方护盾
    │           └── 只播放特效，不造成伤害
    │
    ├── 检查目标有效性
    │       └── 根据 SkillTargetType 判断是否为有效目标
    │
    └── 根据 DamageLogic 处理
            ├── Normal: 碰撞触发 + 间隔触发
            ├── OnlyCollision: 仅碰撞触发
            └── DurationAfterDamage: 持续时间结束后触发
```

### 7.3 HitContext 命中上下文

```csharp
public class HitContext
{
    public ChampionController Caster;        // 施法者
    public ChampionController Target;        // 被命中目标
    public SkillData SkillData;              // 技能数据
    public Vector3 HitPosition;              // 命中位置
    public float HitEffectDuration = 1.5f;   // 命中特效持续时间
    public bool OnlyEffect;                  // 是否只播放特效
    public Collider HitCollider;             // 碰撞体
}
```

---

## 8. VFX资源管理

### 8.1 资源路径规范

VFX资源存放路径：`Resources/Prefab/Projectile/Skill/{skillID}/`

每个技能包含三种特效：
- `Emit`: 发射特效
- `Effect`: 效果特效（投射物）
- `Hit`: 命中特效

### 8.2 VFXBundle 资源包

```csharp
public class VFXBundle
{
    public GameObject EmitPrefab;    // 发射特效预制体
    public GameObject EffectPrefab;  // 效果特效预制体
    public GameObject HitPrefab;     // 命中特效预制体
    
    public bool IsLoaded => EmitPrefab != null || EffectPrefab != null || HitPrefab != null;
}
```

### 8.3 SkillVFXLoader 使用方式

```csharp
// 单例访问
var loader = SkillVFXLoader.Instance;

// 战斗前预加载
loader.PreloadForBattle(skillIds);

// 获取特效预制体
var emitPrefab = loader.GetEmitPrefab(skillId);
var effectPrefab = loader.GetEffectPrefab(skillId);
var hitPrefab = loader.GetHitPrefab(skillId);

// 清除缓存
loader.ClearCache();
loader.ClearCache(skillId);
```

---

## 9. 配置数据结构

### 9.1 SkillData 字段说明

基于 Excel 配置生成的 `SkillData` 类：

| 字段 | 类型 | 说明 |
|------|------|------|
| `ID` | int | 技能唯一ID |
| `name` | string | 技能名称 |
| `index` | int | 技能索引 |
| `Level` | int | 技能等级 |
| `DPS` | float | 每秒伤害 |
| `Dmg` | float | 单次伤害 |
| `delay` | float | 延迟时间 |
| `duration` | float | 持续时间 |
| `effectCounts` | int | 效果触发次数 |
| `isDirectEffect` | bool | 是否为直接效果（无投射物） |
| `castDelay` | float | 施法延迟 |
| `chargingDelay` | float | 充能延迟 |
| `manaCost` | int | 法力消耗 |
| `usableCount` | int | 可使用次数 (-1为无限) |
| `description` | string | 技能描述 |
| `distance` | int | 射程 |
| `range` | int | 效果范围 |
| `skillTargetType` | string | 目标类型 |
| `skillTargetSelectorType` | string | 目标选择策略 |
| `skillRangeSelectorType` | string | 范围选择类型 |
| `damageData` | damageDataClass[] | 伤害数据数组 |
| `skillDecorators` | string[] | 技能装饰器 |
| `paramValues` | paramValuesClass[] | 自定义参数 |
| `addBuffs` | int[] | 附加Buff ID列表 |
| `isBlockOther` | bool | 是否阻挡其他技能（脱手） |
| `skillAnimTrigger` | skillAnimTriggerClass[] | 动画触发器 |
| `emitFXPrefab` | bool | 是否有发射特效 |
| `effectPrefab` | bool | 是否有效果特效 |
| `hitFXPrefab` | bool | 是否有命中特效 |
| `hexEffectPrefab` | string | 棋格效果预制体 |
| `icon` | string | 图标路径 |
| `AttackType` | int | 攻击类型 (对应 SkillAttackType) |
| `MoveSpeed` | float | 弹道移动速度 |

### 9.2 damageDataClass 伤害数据

```csharp
public class damageDataClass
{
    public int dmg;           // 伤害值
    public float correction;  // 修正系数
    public string type;       // 伤害类型
}
```

### 9.3 skillAnimTriggerClass 动画触发器

```csharp
public class skillAnimTriggerClass
{
    public string constructorType;  // 部件类型
    public string trigger;          // 动画触发器名称
}
```

---

## 10. 扩展指南

### 10.1 添加新的弹道类型

**步骤 1: 定义攻击类型枚举**

在 `SkillDefine.cs` 的 `SkillHelper.SkillAttackType` 中添加新枚举：

```csharp
public enum SkillAttackType
{
    // ... 现有类型
    NewTrajectoryType = 11,  // 新弹道类型
}
```

**步骤 2: 配置逻辑数据映射**

在 `SkillHelper.AllLogicDataDic` 中添加映射：

```csharp
{
    SkillAttackType.NewTrajectoryType,
    new SkillLogicData() 
    { 
        MoveLogic = MoveLogic.YourMoveLogic, 
        DamageLogic = DamageLogic.YourDamageLogic,
        DestroyOnColliderShield = false,
        IsCreateInSelf = false
    }
}
```

**步骤 3: 实现移动逻辑（如需要）**

在 `SkillMoveMethods.cs` 中添加新的移动逻辑：

```csharp
public enum MoveLogic
{
    // ... 现有类型
    YourMoveLogic = 9,
}

// 在 SkillMove 方法中添加 case
case SkillHelper.MoveLogic.YourMoveLogic:
    YourMoveMethod(instance, self, target);
    break;

private static void YourMoveMethod(SkillInstance instance, Transform self, Transform target)
{
    // 实现移动逻辑
}
```

---

### 10.2 添加新的目标选择策略

**步骤 1: 定义枚举**

在 `SkillTargetsSelector.cs` 中添加：

```csharp
public enum SkillTargetSelectorType
{
    // ... 现有类型
    YourSelectorType,
}
```

**步骤 2: 实现选择逻辑**

在 `SkillTargetsSelector.FindTargetBySelectorType` 方法中添加 case：

```csharp
case SkillTargetSelectorType.YourSelectorType:
    // 实现选择逻辑
    c = targetList.Where(t => /* 你的条件 */).FirstOrDefault();
    break;
```

---

### 10.3 添加新的伤害逻辑

**步骤 1: 定义枚举**

在 `SkillDefine.cs` 的 `SkillHelper.DamageLogic` 中添加：

```csharp
public enum DamageLogic
{
    // ... 现有类型
    YourDamageLogic = 4,
}
```

**步骤 2: 实现伤害逻辑**

在 `SkillDamageMethods.cs` 中添加处理方法：

```csharp
// 在 SkillDamage 方法中添加 case
case SkillHelper.DamageLogic.YourDamageLogic:
    YourDamageMethod(instance, self, championController, target, colliderType);
    break;

private static void YourDamageMethod(SkillInstance instance, Transform self, 
    ChampionController championController, Collider target, ColliderType colliderType)
{
    // 实现伤害逻辑
}
```

---

### 10.4 创建自定义技能类

如果需要完全自定义的技能行为，可以继承 `SkillBase`：

```csharp
public class CustomSkill : SkillBase
{
    public CustomSkill(SkillData skillData, ChampionController championController, 
        ConstructorBase constructor) : base(skillData, championController, constructor)
    {
    }

    // 重写需要自定义的方法
    public override void ResetSkillContext(bool isResetCount = false)
    {
        base.ResetSkillContext(isResetCount);
        // 自定义初始化逻辑
    }
}
```

然后在 `SkillHelper.IndexToSkillType` 中注册：

```csharp
public static Dictionary<int, System.Type> IndexToSkillType = new()
{
    { 1, typeof(SkillBase) },
    { 2, typeof(CustomSkill) },  // 新技能类型
};
```

---

## 附录

### A. 常量定义 (SkillConstants)

| 常量 | 值 | 说明 |
|------|-----|------|
| `Max_FindTargetDistance` | 60 | 最大寻敌距离 |
| `DEFAULT_HIT_EFFECT_DURATION` | 1.5f | 默认命中特效持续时间 |
| `DEFAULT_EMIT_EFFECT_DURATION` | 1.5f | 默认发射特效持续时间 |
| `DEFAULT_SWEEP_MAX_ANGLE` | 30f | 默认扫射最大角度 |
| `FULL_ROTATION_ANGLE` | 360f | 完整圆周角度 |
| `DEFAULT_MAX_PROJECTILE_DURATION` | 5f | 默认投射物最大飞行时间 |
| `ROCKET_MIN_HEIGHT` | 3f | 火箭最小高度 |
| `ROCKET_MAX_HEIGHT` | 7f | 火箭最大高度 |
| `ROCKET_MIN_HORIZONTAL_OFFSET` | -3f | 火箭最小水平偏移 |
| `ROCKET_MAX_HORIZONTAL_OFFSET` | 3f | 火箭最大水平偏移 |
| `ROCKET_RISE_DURATION` | 0.5f | 火箭上升阶段时间 |
| `ROCKET_TRACK_DURATION` | 1f | 火箭追踪阶段时间 |
| `BEZIER_CONTROL_POINT_1_HEIGHT` | 5f | 贝塞尔曲线控制点1高度 |
| `BEZIER_CONTROL_POINT_2_HEIGHT` | 2f | 贝塞尔曲线控制点2高度 |
| `BEZIER_SAMPLE_POINTS` | 5 | 贝塞尔曲线采样点数 |
| `DAMAGE_INTERVAL` | 1f | 持续伤害间隔 |
| `MOVE_SCALE_RATE` | 0.3f | 移动缩放速率 |

### B. 文件结构

```
Scripts/Skill/
├── SkillDefine.cs              # 接口、枚举、上下文、工具类、工厂
├── SkillBase.cs                # 技能基类
├── SkillController.cs          # 技能管理器
├── SkillStateMachine.cs        # 技能状态机
├── SkillExeProcess.cs          # 技能执行器
├── SkillRuntime.cs             # 运行时数据
├── SkillInstance.cs            # 投射物实例
├── SkillEffectFactory.cs       # 效果工厂
├── SkillHitHandler.cs          # 命中处理器
├── SkillTargetFinder.cs        # 目标查找器
├── SkillDamageMethods.cs       # 伤害逻辑扩展方法
├── SkillMoveMethods.cs         # 移动逻辑扩展方法
├── SkillMoveContext.cs         # 移动上下文
├── SkillVFXLoader.cs           # VFX资源加载器
├── SkillConstants.cs           # 常量定义
├── ISkillExecutionContext.cs   # 执行上下文接口
└── TargetsSelector/
    └── SkillTargetsSelector.cs # 目标选择器
```

---

> **文档结束**
