using UnityEngine;

/// <summary>
/// 技能测试场景设置工具
/// 在Hierarchy右键菜单中快速创建技能测试环境
/// </summary>
public class SkillTestSceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/Skill Test/Create Skill Test Environment", false, 10)]
    static void CreateSkillTestEnvironment(UnityEditor.MenuCommand menuCommand)
    {
        // 创建根对象
        GameObject root = new GameObject("SkillTestEnvironment");
        
        // 添加测试控制器
        var controller = root.AddComponent<SkillTestController>();
        
        // 创建施法点
        GameObject casterSpawnPoint = new GameObject("CasterSpawnPoint");
        casterSpawnPoint.transform.parent = root.transform;
        casterSpawnPoint.transform.localPosition = Vector3.zero;
        controller.casterSpawnPoint = casterSpawnPoint.transform;
        
        // 设置默认值
        controller.autoGenerateTargets = true;
        controller.autoTargetCount = 3;
        controller.targetSpawnRadius = 8f;
        controller.showDebugInfo = true;
        
        // 创建地面参考
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = root.transform;
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        
        // 设置地面材质为半透明
        var renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            renderer.material = material;
        }
        
        // 注册撤销
        UnityEditor.Undo.RegisterCreatedObjectUndo(root, "Create Skill Test Environment");
        
        // 选中创建的对象
        UnityEditor.Selection.activeGameObject = root;
        
        Debug.Log("[SkillTest] 技能测试环境已创建。请进入Play模式后点击 '释放技能' 按钮进行测试。");
    }

    [UnityEditor.MenuItem("GameObject/Skill Test/Add Test Target", false, 11)]
    static void AddTestTarget(UnityEditor.MenuCommand menuCommand)
    {
        // 创建目标
        GameObject targetGO = new GameObject("TestTarget");
        
        // 如果有选中的对象，作为其子对象
        if (UnityEditor.Selection.activeGameObject != null)
        {
            targetGO.transform.parent = UnityEditor.Selection.activeGameObject.transform;
        }
        
        targetGO.transform.localPosition = Vector3.forward * 5f;
        
        // 添加碰撞器
        var collider = targetGO.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = Vector3.up;
        
        // 添加可视化
        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        visual.transform.parent = targetGO.transform;
        visual.transform.localPosition = Vector3.up;
        visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        // 移除可视化对象的碰撞器（避免重复碰撞）
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        
        // 设置为敌人标签
        targetGO.tag = "Enemy";
        
        // 添加测试目标组件
        var target = targetGO.AddComponent<SkillTestTarget>();
        target.Initialize();
        
        // 注册撤销
        UnityEditor.Undo.RegisterCreatedObjectUndo(targetGO, "Add Test Target");
        
        // 选中创建的对象
        UnityEditor.Selection.activeGameObject = targetGO;
    }
#endif
}
