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
    public string description;
    
    public EnemyAction(ActionType type, int value, string description = "")
    {
        this.type = type;
        this.value = value;
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
    
    // AI 패턴 (행동 목록)
    public List<EnemyAction> actionPattern = new List<EnemyAction>();
    
    public EnemyData(string name, int health, int minDmg, int maxDmg)
    {
        enemyName = name;
        maxHealth = health;
        minDamage = minDmg;
        maxDamage = maxDmg;
    }
}