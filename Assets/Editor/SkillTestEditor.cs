using UnityEngine;
using UnityEditor;
using ExcelConfig;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 技能测试控制器的自定义编辑器
/// 提供更友好的技能测试界面
/// </summary>
[CustomEditor(typeof(SkillTestController))]
public class SkillTestEditor : Editor
{
    private SkillTestController _controller;
    private SerializedProperty _skillIDProp;
    private SerializedProperty _targetsProp;
    
    // 技能选择相关
    private int _selectedSkillIndex = 0;
    private string[] _skillNames;
    private int[] _skillIDs;
    private bool _skillListLoaded = false;
    
    // 折叠状态
    private bool _showSkillConfig = true;
    private bool _showOverrides = false;
    private bool _showTargets = true;
    private bool _showCaster = true;
    private bool _showControl = true;
    private bool _showStatus = true;
    private bool _showQuickActions = true;
    
    // 样式
    private GUIStyle _headerStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _statusStyle;
    private bool _stylesInitialized = false;

    private void OnEnable()
    {
        _controller = (SkillTestController)target;
        _skillIDProp = serializedObject.FindProperty("skillID");
        _targetsProp = serializedObject.FindProperty("targets");
        
        LoadSkillList();
    }

    private void InitStyles()
    {
        if (_stylesInitialized) return;
        
        _headerStyle = new GUIStyle(EditorStyles.foldoutHeader)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
        
        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fixedHeight = 30
        };
        
        _statusStyle = new GUIStyle(EditorStyles.helpBox)
        {
            fontSize = 11,
            padding = new RectOffset(10, 10, 5, 5)
        };
        
        _stylesInitialized = true;
    }

    private void LoadSkillList()
    {
        if (GameExcelConfig.Instance == null)
        {
            _skillListLoaded = false;
            return;
        }

        var skills = GameExcelConfig.Instance.skillDatasArray;
        if (skills == null || skills.Count == 0)
        {
            _skillListLoaded = false;
            return;
        }

        _skillNames = new string[skills.Count];
        _skillIDs = new int[skills.Count];
        
        for (int i = 0; i < skills.Count; i++)
        {
            _skillNames[i] = $"[{skills[i].ID}] {skills[i].name}";
            _skillIDs[i] = skills[i].ID;
            
            if (skills[i].ID == _controller.skillID)
            {
                _selectedSkillIndex = i;
            }
        }
        
        _skillListLoaded = true;
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        serializedObject.Update();
        
        // 标题
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("🎮 技能测试工具", EditorStyles.largeLabel);
        EditorGUILayout.Space(5);
        
        // 快捷操作按钮
        DrawQuickActions();
        
        EditorGUILayout.Space(10);
        
        // 技能配置
        DrawSkillConfig();
        
        // 参数覆盖
        DrawOverrides();
        
        // 目标设置
        DrawTargetSettings();
        
        // 施法者设置
        DrawCasterSettings();
        
        // 测试控制
        DrawTestControl();
        
        // 运行时状态
        DrawRuntimeStatus();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawQuickActions()
    {
        _showQuickActions = EditorGUILayout.BeginFoldoutHeaderGroup(_showQuickActions, "⚡ 快捷操作");
        if (_showQuickActions)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("▶ 释放技能", _buttonStyle))
            {
                if (Application.isPlaying)
                {
                    _controller.CastSkill();
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请先进入Play模式", "确定");
                }
            }
            
            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            if (GUILayout.Button("■ 停止", _buttonStyle))
            {
                _controller.StopSkillTest();
            }
            
            GUI.backgroundColor = new Color(0.4f, 0.6f, 0.8f);
            if (GUILayout.Button("↻ 重置目标", _buttonStyle))
            {
                _controller.ResetTargets();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("初始化环境", GUILayout.Height(25)))
            {
                _controller.InitializeTestEnvironment();
            }
            
            if (GUILayout.Button("清理环境", GUILayout.Height(25)))
            {
                _controller.CleanupTestEnvironment();
            }
            
            if (GUILayout.Button("重新加载技能", GUILayout.Height(25)))
            {
                LoadSkillList();
                _controller.LoadSkillData();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSkillConfig()
    {
        _showSkillConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_showSkillConfig, "🎯 技能配置");
        if (_showSkillConfig)
        {
            EditorGUI.indentLevel++;
            
            // 技能下拉选择
            if (_skillListLoaded && _skillNames != null && _skillNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                _selectedSkillIndex = EditorGUILayout.Popup("选择技能", _selectedSkillIndex, _skillNames);
                if (EditorGUI.EndChangeCheck())
                {
                    _skillIDProp.intValue = _skillIDs[_selectedSkillIndex];
                    if (Application.isPlaying)
                    {
                        _controller.LoadSkillData();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("技能配置未加载。请确保GameExcelConfig已初始化。", MessageType.Warning);
                EditorGUILayout.PropertyField(_skillIDProp, new GUIContent("技能ID"));
            }
            
            // 显示技能信息
            if (_skillListLoaded && _selectedSkillIndex >= 0 && _selectedSkillIndex < _skillIDs.Length)
            {
                var skillData = GameExcelConfig.Instance?.skillDatasArray?.Find(s => s.ID == _skillIDs[_selectedSkillIndex]);
                if (skillData != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("技能信息", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"名称: {skillData.name}");
                    EditorGUILayout.LabelField($"持续时间: {skillData.duration}s");
                    EditorGUILayout.LabelField($"生效次数: {skillData.effectCounts}");
                    EditorGUILayout.LabelField($"射程: {skillData.distance} | 范围: {skillData.range}");
                    EditorGUILayout.LabelField($"蓝耗: {skillData.manaCost}");
                    EditorGUILayout.LabelField($"攻击类型: {(SkillHelper.SkillAttackType)skillData.AttackType}");
                    if (!string.IsNullOrEmpty(skillData.description))
                    {
                        EditorGUILayout.LabelField($"描述: {skillData.description}", EditorStyles.wordWrappedLabel);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoLoadFromConfig"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawOverrides()
    {
        _showOverrides = EditorGUILayout.BeginFoldoutHeaderGroup(_showOverrides, "🔧 参数覆盖（测试用）");
        if (_showOverrides)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableOverride"), new GUIContent("启用覆盖"));
            
            if (_controller.enableOverride)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("以下参数将覆盖配置表中的值", MessageType.Info);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDuration"), new GUIContent("持续时间"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideEffectCounts"), new GUIContent("生效次数"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDistance"), new GUIContent("射程"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideRange"), new GUIContent("范围"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideMoveSpeed"), new GUIContent("移动速度"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideAttackType"), new GUIContent("攻击类型"));
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTargetSettings()
    {
        _showTargets = EditorGUILayout.BeginFoldoutHeaderGroup(_showTargets, "🎯 目标设置");
        if (_showTargets)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoGenerateTargets"));
            
            if (_controller.autoGenerateTargets)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoTargetCount"), new GUIContent("目标数量"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetSpawnRadius"), new GUIContent("生成半径"));
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_targetsProp, new GUIContent("目标列表"), true);
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawCasterSettings()
    {
        _showCaster = EditorGUILayout.BeginFoldoutHeaderGroup(_showCaster, "🤖 施法者设置");
        if (_showCaster)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("casterPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("casterSpawnPoint"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTestControl()
    {
        _showControl = EditorGUILayout.BeginFoldoutHeaderGroup(_showControl, "⚙️ 测试控制");
        if (_showControl)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopInterval"), new GUIContent("循环间隔(秒)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugInfo"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRuntimeStatus()
    {
        _showStatus = EditorGUILayout.BeginFoldoutHeaderGroup(_showStatus, "📊 运行时状态");
        if (_showStatus)
        {
            EditorGUILayout.BeginVertical(_statusStyle);
            
            var isRunning = serializedObject.FindProperty("isTestRunning").boolValue;
            var phase = (SkillPhase)serializedObject.FindProperty("currentPhase").enumValueIndex;
            var duration = serializedObject.FindProperty("currentDuration").floatValue;
            var effectCount = serializedObject.FindProperty("currentEffectCount").intValue;
            var skillName = serializedObject.FindProperty("currentSkillName").stringValue;
            
            // 状态指示器
            GUI.color = isRunning ? Color.green : Color.gray;
            EditorGUILayout.LabelField($"● 状态: {(isRunning ? "运行中" : "空闲")}");
            GUI.color = Color.white;
            
            EditorGUILayout.LabelField($"当前技能: {skillName}");
            EditorGUILayout.LabelField($"执行阶段: {phase}");
            EditorGUILayout.LabelField($"持续时间: {duration:F2}s");
            EditorGUILayout.LabelField($"生效次数: {effectCount}");
            
            EditorGUILayout.EndVertical();
            
            // 如果正在运行，刷新Inspector
            if (isRunning)
            {
                Repaint();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}

/// <summary>
/// 技能测试目标的自定义编辑器
/// </summary>
[CustomEditor(typeof(SkillTestTarget))]
public class SkillTestTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var target = (SkillTestTarget)this.target;
        
        EditorGUILayout.LabelField("🎯 技能测试目标", EditorStyles.largeLabel);
        EditorGUILayout.Space(5);
        
        // 快捷操作
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("重置状态", GUILayout.Height(25)))
        {
            target.ResetState();
        }
        if (GUILayout.Button("清空日志", GUILayout.Height(25)))
        {
            target.ClearDamageLog();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 绘制默认Inspector
        DrawDefaultInspector();
        
        // 显示统计信息
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("📊 统计信息", EditorStyles.boldLabel);
        
        float healthRatio = target.maxHealth > 0 ? target.currentHealth / target.maxHealth : 0;
        EditorGUILayout.LabelField($"存活状态: {(target.IsAlive ? "存活" : "死亡")}");
        
        // 血条
        Rect healthRect = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.DrawRect(healthRect, Color.gray);
        healthRect.width *= healthRatio;
        EditorGUI.DrawRect(healthRect, Color.Lerp(Color.red, Color.green, healthRatio));
        
        EditorGUILayout.LabelField($"总受伤: {target.TotalDamageTaken:F0}");
        EditorGUILayout.LabelField($"受击次数: {target.HitCount}");
        
        EditorGUILayout.EndVertical();
    }
}
