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
        // 약한 적
        EnemyData weakEnemy = new EnemyData("슬라임", 150, 35, 45, 5);
        weakEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 40, 15, "체당 공격"));
        weakEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 35, 12, "약한 공격"));
        weakEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 20, 0, "방어"));
        enemies.Add("슬라임", weakEnemy);
        
        // 중간 적
        EnemyData mediumEnemy = new EnemyData("오크", 250, 55, 65, 10);
        mediumEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 60, 25, "강타"));
        mediumEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 55, 20, "일반 공격"));
        mediumEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 30, 0, "방어 태세"));
        mediumEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 70, 30, "분노의 일격"));
        enemies.Add("오크", mediumEnemy);
        
        // 강한 적
        EnemyData strongEnemy = new EnemyData("트롤", 350, 75, 85, 15);
        strongEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 80, 35, "내려치기"));
        strongEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 75, 30, "휘두르기"));
        strongEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 40, 0, "재생"));
        strongEnemy.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 90, 40, "광폭화"));
        enemies.Add("트롤", strongEnemy);
        
        // 엘리트
        EnemyData elite = new EnemyData("오우거", 500, 95, 105, 20);
        elite.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 100, 40, "강력한 일격"));
        elite.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 90, 35, "연속 공격"));
        elite.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 50, 0, "철벽 방어"));
        elite.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 110, 45, "필살기"));
        enemies.Add("오우거", elite);
        
        // 보스
        EnemyData boss = new EnemyData("드래곤", 800, 110, 130, 30);
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 120, 50, "화염 브레스"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 100, 40, "발톱 공격"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Defend, 60, 0, "비늘 강화"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 110, 45, "꼬리 휩쓸기"));
        boss.actionPattern.Add(new EnemyAction(EnemyAction.ActionType.Attack, 140, 60, "분노의 포효"));
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
        return enemies["슬라임"]; // 기본값
    }
    
    public EnemyData GetRandomEnemy()
    {
        string[] enemyNames = { "슬라임", "오크", "트롤" };
        string randomName = enemyNames[Random.Range(0, enemyNames.Length)];
        return GetEnemy(randomName);
    }
}