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
    private SerializedProperty _testModeProp;
    private SerializedProperty _skillChainIDsProp;
    
    // 技能选择相关
    private int _selectedSkillIndex = 0;
    private string[] _skillNames;
    private int[] _skillIDs;
    private bool _skillListLoaded = false;
    
    // 技能链选择相关
    private int _chainSkillToAdd = 0;
    
    // 折叠状态
    private bool _showSkillConfig = true;
    private bool _showSkillChainConfig = true;
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
        _testModeProp = serializedObject.FindProperty("testMode");
        _skillChainIDsProp = serializedObject.FindProperty("skillChainIDs");
        
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
        
        // 测试模式选择
        DrawTestModeSelector();
        
        EditorGUILayout.Space(5);
        
        // 快捷操作按钮
        DrawQuickActions();
        
        EditorGUILayout.Space(10);
        
        // 根据模式显示不同配置
        if (_controller.testMode == SkillTestMode.SingleSkill)
        {
            // 技能配置
            DrawSkillConfig();
        }
        else
        {
            // 技能链配置
            DrawSkillChainConfig();
        }
        
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
    
    private void DrawTestModeSelector()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("📋 测试模式", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_testModeProp, new GUIContent("模式选择"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying)
            {
                _controller.LoadSkillData();
            }
        }
        
        // 显示模式说明
        if (_controller.testMode == SkillTestMode.SingleSkill)
        {
            EditorGUILayout.HelpBox("单技能模式：测试单个技能的效果", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("技能链模式：按顺序测试多个技能，模拟实际战斗中的技能链释放", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawQuickActions()
    {
        _showQuickActions = EditorGUILayout.BeginFoldoutHeaderGroup(_showQuickActions, "⚡ 快捷操作");
        if (_showQuickActions)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 根据模式显示不同的按钮文字
            string castButtonText = _controller.testMode == SkillTestMode.SingleSkill 
                ? "▶ 释放技能" 
                : "▶ 释放技能链";
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button(castButtonText, _buttonStyle))
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
    
    private void DrawSkillChainConfig()
    {
        _showSkillChainConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_showSkillChainConfig, "🔗 技能链配置");
        if (_showSkillChainConfig)
        {
            EditorGUI.indentLevel++;
            
            // 技能链列表
            EditorGUILayout.LabelField("技能链列表（按顺序执行）", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (_controller.skillChainIDs.Count == 0)
            {
                EditorGUILayout.HelpBox("技能链为空，请添加技能", MessageType.Warning);
            }
            else
            {
                // 显示当前技能链
                for (int i = 0; i < _controller.skillChainIDs.Count; i++)
                {
                    int skillId = _controller.skillChainIDs[i];
                    string skillName = GetSkillNameById(skillId);
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    // 序号
                    EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(25));
                    
                    // 技能名称
                    EditorGUILayout.LabelField($"[{skillId}] {skillName}", GUILayout.ExpandWidth(true));
                    
                    // 上移按钮
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("↑", GUILayout.Width(25)))
                    {
                        MoveSkillInChain(i, i - 1);
                    }
                    
                    // 下移按钮
                    GUI.enabled = i < _controller.skillChainIDs.Count - 1;
                    if (GUILayout.Button("↓", GUILayout.Width(25)))
                    {
                        MoveSkillInChain(i, i + 1);
                    }
                    GUI.enabled = true;
                    
                    // 删除按钮
                    GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                    if (GUILayout.Button("×", GUILayout.Width(25)))
                    {
                        RemoveSkillFromChain(i);
                    }
                    GUI.backgroundColor = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // 添加技能
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            if (_skillListLoaded && _skillNames != null && _skillNames.Length > 0)
            {
                _chainSkillToAdd = EditorGUILayout.Popup("添加技能", _chainSkillToAdd, _skillNames);
                
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button("+ 添加", GUILayout.Width(60)))
                {
                    AddSkillToChain(_skillIDs[_chainSkillToAdd]);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("技能配置未加载", MessageType.Warning);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 快捷操作
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("清空技能链"))
            {
                ClearSkillChain();
            }
            
            if (GUILayout.Button("加载技能链数据"))
            {
                _controller.LoadSkillChainData();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 技能链参数
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("链条参数", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chainLoopCount"), new GUIContent("循环次数 (0=无限)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillInterval"), new GUIContent("技能间隔(秒)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chainLoopInterval"), new GUIContent("循环间隔(秒)"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoLoadFromConfig"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
    private string GetSkillNameById(int skillId)
    {
        if (!_skillListLoaded || GameExcelConfig.Instance == null)
            return "未知";
            
        var skill = GameExcelConfig.Instance.skillDatasArray?.Find(s => s.ID == skillId);
        return skill?.name ?? "未知";
    }
    
    private void AddSkillToChain(int skillId)
    {
        Undo.RecordObject(_controller, "Add Skill to Chain");
        _controller.skillChainIDs.Add(skillId);
        EditorUtility.SetDirty(_controller);
    }
    
    private void RemoveSkillFromChain(int index)
    {
        if (index >= 0 && index < _controller.skillChainIDs.Count)
        {
            Undo.RecordObject(_controller, "Remove Skill from Chain");
            _controller.skillChainIDs.RemoveAt(index);
            EditorUtility.SetDirty(_controller);
        }
    }
    
    private void MoveSkillInChain(int fromIndex, int toIndex)
    {
        if (fromIndex >= 0 && fromIndex < _controller.skillChainIDs.Count &&
            toIndex >= 0 && toIndex < _controller.skillChainIDs.Count)
        {
            Undo.RecordObject(_controller, "Move Skill in Chain");
            int temp = _controller.skillChainIDs[fromIndex];
            _controller.skillChainIDs[fromIndex] = _controller.skillChainIDs[toIndex];
            _controller.skillChainIDs[toIndex] = temp;
            EditorUtility.SetDirty(_controller);
        }
    }
    
    private void ClearSkillChain()
    {
        if (EditorUtility.DisplayDialog("确认清空", "确定要清空技能链吗？", "确定", "取消"))
        {
            Undo.RecordObject(_controller, "Clear Skill Chain");
            _controller.skillChainIDs.Clear();
            EditorUtility.SetDirty(_controller);
        }
    }

    private void DrawOverrides()
    {
        _showOverrides = EditorGUILayout.BeginFoldoutHeaderGroup(_showOverrides, "🔧 参数覆盖（测试用）");
        if (_showOverrides)
        {
            EditorGUI.indentLevel++;
            
            // 根据模式显示不同的启用选项
            if (_controller.testMode == SkillTestMode.SingleSkill)
            {
                // 单技能模式 - 检测启用状态变化
                bool wasEnabled = _controller.enableOverride;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableOverride"), new GUIContent("启用覆盖"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    // 从未启用变为启用时，自动从配置加载参数
                    if (!wasEnabled && _controller.enableOverride)
                    {
                        _controller.LoadOverridesFromConfig();
                        EditorUtility.SetDirty(_controller);
                    }
                }
                
                if (_controller.enableOverride)
                {
                    DrawUnifiedOverrideParams();
                }
            }
            else
            {
                // 技能链模式
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chainOverrideMode"), new GUIContent("覆盖模式"));
                
                switch (_controller.chainOverrideMode)
                {
                    case SkillChainOverrideMode.None:
                        EditorGUILayout.HelpBox("不覆盖参数，使用配置表中的值", MessageType.Info);
                        break;
                        
                    case SkillChainOverrideMode.Unified:
                        EditorGUILayout.HelpBox("所有技能使用相同的覆盖参数", MessageType.Warning);
                        DrawUnifiedOverrideParams();
                        break;
                        
                    case SkillChainOverrideMode.Individual:
                        EditorGUILayout.HelpBox("每个技能可单独配置覆盖参数", MessageType.Info);
                        DrawIndividualOverrideParams();
                        break;
                }
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
    private void DrawUnifiedOverrideParams()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("覆盖参数", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDuration"), new GUIContent("持续时间"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideEffectCounts"), new GUIContent("生效次数"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDistance"), new GUIContent("射程"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideRange"), new GUIContent("范围"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideMoveSpeed"), new GUIContent("移动速度"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideAttackType"), new GUIContent("攻击类型"));
        
        EditorGUILayout.EndVertical();
        
        // 保存按钮区域（仅单技能模式）
        if (_controller.testMode == SkillTestMode.SingleSkill)
        {
            DrawSingleSkillSaveButtons();
        }
    }
    
    /// <summary>
    /// 绘制单技能模式的保存按钮
    /// </summary>
    private void DrawSingleSkillSaveButtons()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        
        // 从配置加载按钮
        if (GUILayout.Button("📥 从配置加载", GUILayout.Height(25)))
        {
            _controller.LoadOverridesFromConfig();
            EditorUtility.SetDirty(_controller);
        }
        
        // 保存到 Excel 按钮
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("💾 保存到 Excel", GUILayout.Height(25)))
        {
            SaveCurrentSkillToExcel();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 保存当前技能的覆盖参数到 Excel
    /// </summary>
    private void SaveCurrentSkillToExcel()
    {
        var overrides = _controller.GetCurrentOverrides();
        if (overrides == null)
        {
            EditorUtility.DisplayDialog("提示", "请先启用覆盖参数", "确定");
            return;
        }
        
        int skillID = _controller.GetCurrentSkillID();
        string skillName = GetSkillNameById(skillID);
        
        bool confirm = EditorUtility.DisplayDialog(
            "保存确认",
            $"确定要将当前覆盖参数保存到 Excel 吗？\n\n" +
            $"技能: [{skillID}] {skillName}\n" +
            $"持续时间: {overrides.Duration}\n" +
            $"生效次数: {overrides.EffectCounts}\n" +
            $"射程: {overrides.Distance}\n" +
            $"范围: {overrides.Range}\n" +
            $"移动速度: {overrides.MoveSpeed}\n" +
            $"攻击类型: {overrides.AttackType}\n\n" +
            $"⚠️ 此操作将直接修改 SkillConfig.xlsx 文件",
            "保存", "取消"
        );
        
        if (confirm)
        {
            bool success = SkillTestExcelSaver.SaveOverridesToExcel(skillID, overrides);
            if (success)
            {
                EditorUtility.DisplayDialog("成功", 
                    $"技能 [{skillID}] {skillName} 的参数已保存到 Excel！\n\n" +
                    "请使用 Tools -> EasyExcel -> Import 重新导入以更新运行时数据。", 
                    "确定");
                SkillTestExcelSaver.RefreshExcelAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "保存失败，请查看控制台日志了解详情", "确定");
            }
        }
    }
    
    private void DrawIndividualOverrideParams()
    {
        EditorGUILayout.Space(5);
        
        // 同步和加载按钮行
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔄 同步技能链覆盖配置"))
        {
            _controller.SyncSkillChainOverrides();
            EditorUtility.SetDirty(_controller);
        }
        
        if (GUILayout.Button("📥 从配置加载"))
        {
            _controller.LoadOverridesFromConfig();
            EditorUtility.SetDirty(_controller);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (_controller.skillChainOverrides.Count == 0)
        {
            EditorGUILayout.HelpBox("请先添加技能到技能链，然后点击上方按钮同步配置", MessageType.Warning);
            return;
        }
        
        // 显示每个技能的覆盖配置
        for (int i = 0; i < _controller.skillChainOverrides.Count; i++)
        {
            var item = _controller.skillChainOverrides[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // 技能名称和启用开关 - 检测启用状态变化
            bool wasEnabled = item.enabled;
            item.enabled = EditorGUILayout.ToggleLeft(
                $"[{item.skillID}] {item.skillName}", 
                item.enabled, 
                EditorStyles.boldLabel
            );
            
            // 从未启用变为启用时，自动从配置加载该技能的参数
            if (!wasEnabled && item.enabled)
            {
                LoadSingleSkillOverridesFromConfig(item);
            }
            
            // 单个技能保存按钮
            if (item.enabled)
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button("💾", GUILayout.Width(30)))
                {
                    SaveSingleChainSkillToExcel(item.skillID, item.skillName, item.overrides);
                }
                GUI.backgroundColor = Color.white;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 如果启用，显示覆盖参数
            if (item.enabled)
            {
                EditorGUI.indentLevel++;
                
                item.overrides.Duration = EditorGUILayout.FloatField("持续时间", item.overrides.Duration);
                item.overrides.EffectCounts = EditorGUILayout.IntField("生效次数", item.overrides.EffectCounts);
                item.overrides.Distance = EditorGUILayout.IntField("射程", item.overrides.Distance);
                item.overrides.Range = EditorGUILayout.IntField("范围", item.overrides.Range);
                item.overrides.MoveSpeed = EditorGUILayout.FloatField("移动速度", item.overrides.MoveSpeed);
                item.overrides.AttackType = (SkillHelper.SkillAttackType)EditorGUILayout.EnumPopup("攻击类型", item.overrides.AttackType);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        
        // 批量保存按钮
        EditorGUILayout.Space(5);
        DrawChainBatchSaveButtons();
        
        // 标记修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(_controller);
        }
    }
    
    /// <summary>
    /// 从配置加载单个技能的参数到覆盖设置
    /// </summary>
    private void LoadSingleSkillOverridesFromConfig(SkillChainOverrideItem item)
    {
        if (GameExcelConfig.Instance == null)
        {
            Debug.LogWarning("[SkillTestEditor] GameExcelConfig 未加载");
            return;
        }
        
        var skillData = GameExcelConfig.Instance.skillDatasArray?.Find(s => s.ID == item.skillID);
        if (skillData != null)
        {
            item.overrides.Duration = skillData.duration;
            item.overrides.EffectCounts = skillData.effectCounts;
            item.overrides.Distance = skillData.distance;
            item.overrides.Range = skillData.range;
            item.overrides.MoveSpeed = skillData.MoveSpeed;
            item.overrides.AttackType = (SkillHelper.SkillAttackType)skillData.AttackType;
            
            EditorUtility.SetDirty(_controller);
            Debug.Log($"[SkillTestEditor] 已从配置加载技能 [{item.skillID}] {item.skillName} 的参数");
        }
    }
    
    /// <summary>
    /// 绘制技能链批量保存按钮
    /// </summary>
    private void DrawChainBatchSaveButtons()
    {
        var enabledItems = _controller.GetEnabledChainOverrides();
        int enabledCount = enabledItems.Count;
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = enabledCount > 0;
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        
        if (GUILayout.Button($"💾 批量保存已启用的技能 ({enabledCount})", GUILayout.Height(28)))
        {
            SaveAllEnabledChainSkillsToExcel(enabledItems);
        }
        
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        if (enabledCount == 0)
        {
            EditorGUILayout.HelpBox("请至少启用一个技能的覆盖配置才能保存", MessageType.Info);
        }
    }
    
    /// <summary>
    /// 保存单个技能链项目到 Excel
    /// </summary>
    private void SaveSingleChainSkillToExcel(int skillID, string skillName, SkillTestOverrides overrides)
    {
        bool confirm = EditorUtility.DisplayDialog(
            "保存确认",
            $"确定要将技能 [{skillID}] {skillName} 的覆盖参数保存到 Excel 吗？\n\n" +
            $"持续时间: {overrides.Duration}\n" +
            $"生效次数: {overrides.EffectCounts}\n" +
            $"射程: {overrides.Distance}\n" +
            $"范围: {overrides.Range}\n" +
            $"移动速度: {overrides.MoveSpeed}\n" +
            $"攻击类型: {overrides.AttackType}\n\n" +
            $"⚠️ 此操作将直接修改 SkillConfig.xlsx 文件",
            "保存", "取消"
        );
        
        if (confirm)
        {
            bool success = SkillTestExcelSaver.SaveOverridesToExcel(skillID, overrides);
            if (success)
            {
                EditorUtility.DisplayDialog("成功", 
                    $"技能 [{skillID}] {skillName} 的参数已保存到 Excel！\n\n" +
                    "请使用 Tools -> EasyExcel -> Import 重新导入以更新运行时数据。", 
                    "确定");
                SkillTestExcelSaver.RefreshExcelAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "保存失败，请查看控制台日志了解详情", "确定");
            }
        }
    }
    
    /// <summary>
    /// 批量保存所有已启用的技能链项目到 Excel
    /// </summary>
    private void SaveAllEnabledChainSkillsToExcel(List<(int skillID, SkillTestOverrides overrides)> items)
    {
        if (items == null || items.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有已启用的覆盖配置", "确定");
            return;
        }
        
        // 构建确认信息
        string skillList = "";
        foreach (var (skillID, _) in items)
        {
            string name = GetSkillNameById(skillID);
            skillList += $"  • [{skillID}] {name}\n";
        }
        
        bool confirm = EditorUtility.DisplayDialog(
            "批量保存确认",
            $"确定要将以下 {items.Count} 个技能的覆盖参数保存到 Excel 吗？\n\n" +
            skillList + "\n" +
            $"⚠️ 此操作将直接修改 SkillConfig.xlsx 文件",
            "全部保存", "取消"
        );
        
        if (confirm)
        {
            bool success = SkillTestExcelSaver.SaveMultipleOverridesToExcel(items);
            if (success)
            {
                EditorUtility.DisplayDialog("成功", 
                    $"已成功保存 {items.Count} 个技能的参数到 Excel！\n\n" +
                    "请使用 Tools -> EasyExcel -> Import 重新导入以更新运行时数据。", 
                    "确定");
                SkillTestExcelSaver.RefreshExcelAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "保存失败，请查看控制台日志了解详情", "确定");
            }
        }
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
            
            // 技能链模式下显示额外信息
            if (_controller.testMode == SkillTestMode.SkillChain)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("技能链状态", EditorStyles.boldLabel);
                
                var chainIndex = serializedObject.FindProperty("currentChainIndex").intValue;
                var loopCount = serializedObject.FindProperty("currentLoopCount").intValue;
                var chainStatus = serializedObject.FindProperty("chainStatusInfo").stringValue;
                var chainCastingComplete = serializedObject.FindProperty("chainCastingComplete").boolValue;
                var chainLoopCountSetting = _controller.chainLoopCount;
                
                // 进度条
                if (_controller.skillChainIDs.Count > 0)
                {
                    float progress = chainCastingComplete ? 1f : (float)chainIndex / _controller.skillChainIDs.Count;
                    string progressText = chainCastingComplete 
                        ? "释放完成，效果运行中" 
                        : $"技能 {chainIndex}/{_controller.skillChainIDs.Count}";
                    Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(progressRect, progress, progressText);
                }
                
                string loopDisplay = chainLoopCountSetting == 0 ? "∞" : chainLoopCountSetting.ToString();
                EditorGUILayout.LabelField($"循环进度: {loopCount + 1}/{loopDisplay}");
                
                if (!string.IsNullOrEmpty(chainStatus))
                {
                    // 根据状态显示不同类型的消息框
                    MessageType msgType = chainCastingComplete ? MessageType.Info : MessageType.None;
                    EditorGUILayout.HelpBox(chainStatus, msgType);
                }
            }
            
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
