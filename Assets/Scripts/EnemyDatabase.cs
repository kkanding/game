using UnityEngine;
using System.Collections.Generic;

public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance;
    
    private Dictionary<string, EnemyData> enemies = new Dictionary<string, EnemyData>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemies();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeEnemies()
    {
        // 고블린 (약함)
        EnemyData goblin = new EnemyData("고블린", 10, 5, 8);
        goblin.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 6, "공격"));
        goblin.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 7, "공격"));
        goblin.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 5, "방어"));
        enemies.Add("고블린", goblin);
        
        // 오크 (중간)
        EnemyData orc = new EnemyData("오크", 10, 8, 12);
        orc.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 10, "강타"));
        orc.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 8, "공격"));
        orc.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 8, "방어"));
        orc.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 12, "분노"));
        enemies.Add("오크", orc);
        
        // 트롤 (강함)
        EnemyData troll = new EnemyData("트롤", 10, 10, 15);
        troll.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 14, "내려치기"));
        troll.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 12, "공격"));
        troll.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 12, "재생"));
        troll.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 16, "광폭화"));
        enemies.Add("트롤", troll);
        
        // 보스 (매우 강함)
        EnemyData boss = new EnemyData("드래곤", 120, 15, 20);
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 18, "화염 브레스"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 15, "발톱 공격"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 15, "비늘 강화"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 20, "꼬리 휩쓸기"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 25, "분노의 포효"));
        enemies.Add("드래곤", boss);
        
        Debug.Log($"적 데이터베이스 초기화 완료! {enemies.Count}종");
    }
    
    public EnemyData GetEnemy(string enemyName)
    {
        if (enemies.ContainsKey(enemyName))
        {
            return enemies[enemyName];
        }
        
        Debug.LogWarning($"적 '{enemyName}'을 찾을 수 없습니다!");
        return enemies["고블린"]; // 기본값
    }
    
    public EnemyData GetRandomEnemy()
    {
        string[] enemyNames = { "고블린", "오크", "트롤" };
        string randomName = enemyNames[Random.Range(0, enemyNames.Length)];
        return GetEnemy(randomName);
    }
}