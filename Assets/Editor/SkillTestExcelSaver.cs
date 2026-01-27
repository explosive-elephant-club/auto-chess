#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using UnityEditor;
using ExcelConfig;

/// <summary>
/// 技能测试覆盖参数保存到 Excel 的工具类
/// 直接修改原始 Excel 文件中对应技能的配置
/// 使用动态列查找，自动适应表格结构变化
/// </summary>
public static class SkillTestExcelSaver
{
    // Excel 配置文件相对路径
    private const string SKILL_CONFIG_PATH = "Assets/asoliddev - Auto Chess/Config/SkillConfig.xlsx";
    private const string SHEET_NAME = "SkillData";
    
    // EasyExcel 设置：数据从第4行开始（index=3），第2行是字段名（index=1）
    private const int NAME_ROW_INDEX = 1;  // 字段名行（0-based）
    private const int DATA_START_INDEX = 3; // 数据起始行（0-based）
    
    // 需要写入的字段名称（与 Excel 表头一致）
    private static class ColumnNames
    {
        public const string ID = "ID";
        public const string Name = "name";
        public const string Duration = "duration";
        public const string EffectCounts = "effectCounts";
        public const string Distance = "distance";
        public const string Range = "range";
        public const string AttackType = "AttackType";
        public const string MoveSpeed = "MoveSpeed";
    }
    
    /// <summary>
    /// 列索引缓存（避免每次都重新解析表头）
    /// </summary>
    private class ColumnIndexCache
    {
        public int ID = -1;
        public int Name = -1;
        public int Duration = -1;
        public int EffectCounts = -1;
        public int Distance = -1;
        public int Range = -1;
        public int AttackType = -1;
        public int MoveSpeed = -1;
        
        public bool IsValid => ID >= 0 && Duration >= 0 && EffectCounts >= 0 && 
                               Distance >= 0 && Range >= 0 && AttackType >= 0 && MoveSpeed >= 0;
        
        public string GetMissingColumns()
        {
            var missing = new List<string>();
            if (ID < 0) missing.Add(ColumnNames.ID);
            if (Duration < 0) missing.Add(ColumnNames.Duration);
            if (EffectCounts < 0) missing.Add(ColumnNames.EffectCounts);
            if (Distance < 0) missing.Add(ColumnNames.Distance);
            if (Range < 0) missing.Add(ColumnNames.Range);
            if (AttackType < 0) missing.Add(ColumnNames.AttackType);
            if (MoveSpeed < 0) missing.Add(ColumnNames.MoveSpeed);
            return string.Join(", ", missing);
        }
    }
    
    /// <summary>
    /// 从表头行动态解析列索引
    /// </summary>
    private static ColumnIndexCache ParseColumnIndices(ExcelWorksheet worksheet)
    {
        var cache = new ColumnIndexCache();
        
        // EPPlus 行列是 1-based，NAME_ROW_INDEX 是 0-based
        int headerRow = NAME_ROW_INDEX + 1;
        int endColumn = worksheet.Dimension?.End.Column ?? 0;
        
        Debug.Log($"[SkillTestExcelSaver] 解析表头，行号: {headerRow}，列数: {endColumn}");
        
        for (int col = 1; col <= endColumn; col++)
        {
            var cellValue = worksheet.Cells[headerRow, col].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(cellValue)) continue;
            
            // 匹配字段名（支持带 :key 后缀的情况，如 "ID:key"）
            string fieldName = cellValue.Split(':')[0].Trim();
            
            switch (fieldName)
            {
                case ColumnNames.ID:
                    cache.ID = col;
                    break;
                case ColumnNames.Name:
                    cache.Name = col;
                    break;
                case ColumnNames.Duration:
                    cache.Duration = col;
                    break;
                case ColumnNames.EffectCounts:
                    cache.EffectCounts = col;
                    break;
                case ColumnNames.Distance:
                    cache.Distance = col;
                    break;
                case ColumnNames.Range:
                    cache.Range = col;
                    break;
                case ColumnNames.AttackType:
                    cache.AttackType = col;
                    break;
                case ColumnNames.MoveSpeed:
                    cache.MoveSpeed = col;
                    break;
            }
        }
        
        // 输出解析结果日志
        Debug.Log($"[SkillTestExcelSaver] 列索引解析结果:\n" +
                 $"  ID: {cache.ID}, name: {cache.Name}\n" +
                 $"  duration: {cache.Duration}, effectCounts: {cache.EffectCounts}\n" +
                 $"  distance: {cache.Distance}, range: {cache.Range}\n" +
                 $"  AttackType: {cache.AttackType}, MoveSpeed: {cache.MoveSpeed}");
        
        return cache;
    }
    
    /// <summary>
    /// 保存单个技能的覆盖参数到 Excel
    /// </summary>
    public static bool SaveOverridesToExcel(int skillID, SkillTestOverrides overrides)
    {
        if (overrides == null)
        {
            Debug.LogError("[SkillTestExcelSaver] 覆盖参数为空");
            return false;
        }
        
        string absolutePath = Path.GetFullPath(SKILL_CONFIG_PATH);
        
        if (!File.Exists(absolutePath))
        {
            Debug.LogError($"[SkillTestExcelSaver] 找不到 Excel 文件: {absolutePath}");
            return false;
        }
        
        try
        {
            var fileInfo = new FileInfo(absolutePath);
            
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[SHEET_NAME];
                if (worksheet == null)
                {
                    Debug.LogError($"[SkillTestExcelSaver] 找不到工作表: {SHEET_NAME}");
                    return false;
                }
                
                // 动态解析列索引
                var columnCache = ParseColumnIndices(worksheet);
                if (!columnCache.IsValid)
                {
                    Debug.LogError($"[SkillTestExcelSaver] 无法找到必要的列: {columnCache.GetMissingColumns()}");
                    return false;
                }
                
                // 查找技能所在行
                int targetRow = FindSkillRow(worksheet, skillID, columnCache.ID);
                if (targetRow == -1)
                {
                    Debug.LogError($"[SkillTestExcelSaver] 在 Excel 中找不到技能 ID: {skillID}");
                    return false;
                }
                
                // 获取技能名称用于日志
                string skillName = columnCache.Name > 0 
                    ? worksheet.Cells[targetRow, columnCache.Name].Value?.ToString() ?? "未知"
                    : "未知";
                
                // 更新单元格值
                worksheet.Cells[targetRow, columnCache.Duration].Value = overrides.Duration;
                worksheet.Cells[targetRow, columnCache.EffectCounts].Value = overrides.EffectCounts;
                worksheet.Cells[targetRow, columnCache.Distance].Value = overrides.Distance;
                worksheet.Cells[targetRow, columnCache.Range].Value = overrides.Range;
                worksheet.Cells[targetRow, columnCache.AttackType].Value = (int)overrides.AttackType;
                worksheet.Cells[targetRow, columnCache.MoveSpeed].Value = overrides.MoveSpeed;
                
                // 保存文件
                package.Save();
                
                Debug.Log($"[SkillTestExcelSaver] ✓ 成功保存技能 [{skillID}] {skillName} 的覆盖参数到 Excel\n" +
                         $"  Duration: {overrides.Duration} (列{columnCache.Duration})\n" +
                         $"  EffectCounts: {overrides.EffectCounts} (列{columnCache.EffectCounts})\n" +
                         $"  Distance: {overrides.Distance} (列{columnCache.Distance})\n" +
                         $"  Range: {overrides.Range} (列{columnCache.Range})\n" +
                         $"  AttackType: {overrides.AttackType} (列{columnCache.AttackType})\n" +
                         $"  MoveSpeed: {overrides.MoveSpeed} (列{columnCache.MoveSpeed})");
                
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillTestExcelSaver] 保存失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 批量保存多个技能的覆盖参数
    /// </summary>
    public static bool SaveMultipleOverridesToExcel(List<(int skillID, SkillTestOverrides overrides)> items)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("[SkillTestExcelSaver] 没有要保存的项目");
            return false;
        }
        
        string absolutePath = Path.GetFullPath(SKILL_CONFIG_PATH);
        
        if (!File.Exists(absolutePath))
        {
            Debug.LogError($"[SkillTestExcelSaver] 找不到 Excel 文件: {absolutePath}");
            return false;
        }
        
        try
        {
            var fileInfo = new FileInfo(absolutePath);
            int savedCount = 0;
            
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[SHEET_NAME];
                if (worksheet == null)
                {
                    Debug.LogError($"[SkillTestExcelSaver] 找不到工作表: {SHEET_NAME}");
                    return false;
                }
                
                // 动态解析列索引（只解析一次）
                var columnCache = ParseColumnIndices(worksheet);
                if (!columnCache.IsValid)
                {
                    Debug.LogError($"[SkillTestExcelSaver] 无法找到必要的列: {columnCache.GetMissingColumns()}");
                    return false;
                }
                
                foreach (var (skillID, overrides) in items)
                {
                    if (overrides == null) continue;
                    
                    int targetRow = FindSkillRow(worksheet, skillID, columnCache.ID);
                    if (targetRow == -1)
                    {
                        Debug.LogWarning($"[SkillTestExcelSaver] 跳过：找不到技能 ID {skillID}");
                        continue;
                    }
                    
                    // 更新单元格值
                    worksheet.Cells[targetRow, columnCache.Duration].Value = overrides.Duration;
                    worksheet.Cells[targetRow, columnCache.EffectCounts].Value = overrides.EffectCounts;
                    worksheet.Cells[targetRow, columnCache.Distance].Value = overrides.Distance;
                    worksheet.Cells[targetRow, columnCache.Range].Value = overrides.Range;
                    worksheet.Cells[targetRow, columnCache.AttackType].Value = (int)overrides.AttackType;
                    worksheet.Cells[targetRow, columnCache.MoveSpeed].Value = overrides.MoveSpeed;
                    
                    savedCount++;
                }
                
                // 保存文件
                package.Save();
                
                Debug.Log($"[SkillTestExcelSaver] ✓ 成功保存 {savedCount}/{items.Count} 个技能的覆盖参数到 Excel");
                return savedCount > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillTestExcelSaver] 批量保存失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 在工作表中查找技能所在行（返回 1-based 行号）
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="skillID">要查找的技能ID</param>
    /// <param name="idColumn">ID列的索引（1-based）</param>
    private static int FindSkillRow(ExcelWorksheet worksheet, int skillID, int idColumn)
    {
        // EPPlus 的行列都是 1-based
        int startRow = DATA_START_INDEX + 1; // 转换为 1-based
        int endRow = worksheet.Dimension?.End.Row ?? 0;
        
        for (int row = startRow; row <= endRow; row++)
        {
            var cellValue = worksheet.Cells[row, idColumn].Value;
            if (cellValue != null)
            {
                if (int.TryParse(cellValue.ToString(), out int id) && id == skillID)
                {
                    return row;
                }
            }
        }
        
        return -1;
    }
    
    /// <summary>
    /// 从当前技能配置创建覆盖参数对象
    /// </summary>
    public static SkillTestOverrides CreateOverridesFromSkillData(SkillData skillData)
    {
        if (skillData == null) return null;
        
        return new SkillTestOverrides
        {
            Duration = skillData.duration,
            EffectCounts = skillData.effectCounts,
            Distance = skillData.distance,
            Range = skillData.range,
            AttackType = (SkillHelper.SkillAttackType)skillData.AttackType,
            MoveSpeed = skillData.MoveSpeed
        };
    }
    
    /// <summary>
    /// 刷新 ScriptableObject 资源（保存 Excel 后调用）
    /// </summary>
    public static void RefreshExcelAssets()
    {
        AssetDatabase.Refresh();
        Debug.Log("[SkillTestExcelSaver] 请使用 Tools -> EasyExcel -> Import 重新导入 Excel 以更新运行时数据");
    }
    
    /// <summary>
    /// 打开 Excel 配置文件
    /// </summary>
    [MenuItem("Tools/技能测试/打开 SkillConfig.xlsx")]
    public static void OpenSkillConfigExcel()
    {
        string absolutePath = Path.GetFullPath(SKILL_CONFIG_PATH);
        if (File.Exists(absolutePath))
        {
            System.Diagnostics.Process.Start(absolutePath);
        }
        else
        {
            EditorUtility.DisplayDialog("错误", $"找不到文件: {absolutePath}", "确定");
        }
    }
    
    /// <summary>
    /// 调试工具：打印 Excel 表头结构
    /// </summary>
    [MenuItem("Tools/技能测试/打印 Excel 表头结构")]
    public static void PrintExcelHeaderStructure()
    {
        string absolutePath = Path.GetFullPath(SKILL_CONFIG_PATH);
        
        if (!File.Exists(absolutePath))
        {
            Debug.LogError($"找不到 Excel 文件: {absolutePath}");
            return;
        }
        
        try
        {
            var fileInfo = new FileInfo(absolutePath);
            
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[SHEET_NAME];
                if (worksheet == null)
                {
                    Debug.LogError($"找不到工作表: {SHEET_NAME}");
                    return;
                }
                
                int headerRow = NAME_ROW_INDEX + 1;
                int endColumn = worksheet.Dimension?.End.Column ?? 0;
                
                string headerInfo = $"Excel 表头结构 (第{headerRow}行):\n";
                for (int col = 1; col <= endColumn; col++)
                {
                    var cellValue = worksheet.Cells[headerRow, col].Value?.ToString() ?? "(空)";
                    headerInfo += $"  列{col}: {cellValue}\n";
                }
                
                Debug.Log(headerInfo);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"读取失败: {e.Message}");
        }
    }
}
#endif
