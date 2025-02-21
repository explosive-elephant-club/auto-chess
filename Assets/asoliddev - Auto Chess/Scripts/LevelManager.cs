using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

public class EnemyConfig
{
    public ChampionController championController;
    public int[] skillIDs;
    public Vector3 pos;
}

public class LevelManager : CreateSingleton<LevelManager>
{
    // Start is called before the first frame update
    protected override void InitSingleton()
    {

    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<EnemyConfig> GetEnemyConfigs()
    {
        List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
        LevelData levelData = GameExcelConfig.Instance._eeDataManager.Get<LevelData>(GameData.Instance.mapLevel);
        foreach (var e in levelData.enemies)
        {
            EnemyData enemyData = GameExcelConfig.Instance._eeDataManager.Get<EnemyData>(e.id);

            GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/" + enemyData.prefab));
            championPrefab.name = championPrefab.name;

            EnemyConfig enemyConfig = new EnemyConfig();
            enemyConfig.championController = championPrefab.GetComponent<ChampionController>();
            enemyConfig.skillIDs = enemyData.skills;
            enemyConfig.pos = new Vector3(e.x, e.y, e.z);

            enemyConfigs.Add(enemyConfig);
        }
        return enemyConfigs;

    }
}
