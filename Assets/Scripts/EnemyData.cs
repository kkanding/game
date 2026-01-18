using System.Collections.Generic;

[System.Serializable]
public class EnemyAction
{
    public enum ActionType
    {
        Attack,
        Defend,
        Buff,
        Debuff
    }
    
    public ActionType type;
    public int value;
    public int mentalAttackValue = 0; // ← 정신공격력 추가!
    public string description;
    
    // 기존 생성자
    public EnemyAction(ActionType type, int value, string description = "")
    {
        this.type = type;
        this.value = value;
        this.description = description;
    }
    
    // 정신공격력 포함 생성자 추가
    public EnemyAction(ActionType type, int value, int mentalAttack, string description = "")
    {
        this.type = type;
        this.value = value;
        this.mentalAttackValue = mentalAttack;
        this.description = description;
    }
}

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public int maxHealth;
    public int minDamage;
    public int maxDamage;
    public int defensePower = 0; // ← 방어력 추가!
    
    // AI 패턴 (행동 목록)
    public List<EnemyAction> actionPattern = new List<EnemyAction>();
    
    public EnemyData(string name, int health, int minDmg, int maxDmg, int defense = 0)
    {
        enemyName = name;
        maxHealth = health;
        minDamage = minDmg;
        maxDamage = maxDmg;
        defensePower = defense;
    }
}