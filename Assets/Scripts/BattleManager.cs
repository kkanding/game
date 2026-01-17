using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum BattleState
{
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    
    [Header("Battle Settings")]
    public int maxEnergy = 3;
    public int currentEnergy;
    
    [Header("Party (3 Characters)")]
    public List<BattleEntity> party = new List<BattleEntity>();
	
	[Header("Party UI")] // ← 추가!
	public Transform character1UI;
	public Transform character2UI;
	public Transform character3UI;
    
    [Header("Enemy")]
    public BattleEntity enemy;
	
	[Header("Enemy UI")] // ← 추가!
	public Transform enemyUI;
    
    public BattleState currentState;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        StartBattle();
    }
    
    void StartBattle()
	{
		Debug.Log("===== 전투 시작! =====");
		
		// 파티 초기화
		party.Clear();
		
		// GameData에서 캐릭터 정보 가져오기
		if (GameData.Instance != null && GameData.Instance.raidParty.Count >= 3)
		{
			// 전사
			if (character1UI != null)
			{
				BattleEntity warrior = character1UI.gameObject.AddComponent<BattleEntity>();
				warrior.entityName = GameData.Instance.raidParty[0].characterName;
				warrior.maxHealth = GameData.Instance.raidParty[0].maxHealth;
				warrior.currentHealth = GameData.Instance.raidParty[0].maxHealth;
				party.Add(warrior);
			}
			
			// 마법사
			if (character2UI != null)
			{
				BattleEntity mage = character2UI.gameObject.AddComponent<BattleEntity>();
				mage.entityName = GameData.Instance.raidParty[1].characterName;
				mage.maxHealth = GameData.Instance.raidParty[1].maxHealth;
				mage.currentHealth = GameData.Instance.raidParty[1].maxHealth;
				party.Add(mage);
			}
			
			// 도적
			if (character3UI != null)
			{
				BattleEntity rogue = character3UI.gameObject.AddComponent<BattleEntity>();
				rogue.entityName = GameData.Instance.raidParty[2].characterName;
				rogue.maxHealth = GameData.Instance.raidParty[2].maxHealth;
				rogue.currentHealth = GameData.Instance.raidParty[2].maxHealth;
				party.Add(rogue);
			}
		}
		
		// ← 적 생성 수정!
		if (enemyUI != null)
		{
			enemy = enemyUI.gameObject.AddComponent<BattleEntity>();
			
			// EnemyDatabase에서 랜덤 적 선택
			if (EnemyDatabase.Instance != null)
			{
				EnemyData enemyData = EnemyDatabase.Instance.GetRandomEnemy();
				enemy.entityName = enemyData.enemyName;
				enemy.maxHealth = enemyData.maxHealth;
				enemy.currentHealth = enemyData.maxHealth;
				enemy.enemyData = enemyData;
				
				// 첫 번째 행동 결정
				enemy.DecideNextAction();
				
				Debug.Log($"적 생성: {enemy.entityName} (체력: {enemy.maxHealth})");
			}
			else
			{
				// EnemyDatabase 없으면 기본 고블린
				enemy.entityName = "고블린";
				enemy.maxHealth = 30;
				enemy.currentHealth = 30;
			}
		}
		
		// 이벤트 연결
		foreach (var member in party)
		{
			member.OnHealthChanged += (health) => BattleUI.Instance?.UpdatePartyUI();
			member.OnBlockChanged += (block) => BattleUI.Instance?.UpdatePartyUI();
			member.OnDeath += () => CheckPartyWipe();
		}
		
		if (enemy != null)
		{
			enemy.OnHealthChanged += (health) => BattleUI.Instance?.UpdateEnemyUI();
			enemy.OnBlockChanged += (block) => BattleUI.Instance?.UpdateEnemyUI();
		}
		
		currentState = BattleState.PlayerTurn;
		
		// 체력바 초기화
		if (BattleUI.Instance != null)
		{
			for (int i = 0; i < party.Count && i < BattleUI.Instance.partyHealthBars.Length; i++)
			{
				if (BattleUI.Instance.partyHealthBars[i] != null && party[i] != null)
				{
					float ratio = (float)party[i].currentHealth / party[i].maxHealth;
					BattleUI.Instance.partyHealthBars[i].SetHealthImmediate(ratio);
				}
			}
			
			if (BattleUI.Instance.enemyHealthBar != null && enemy != null)
			{
				float ratio = (float)enemy.currentHealth / enemy.maxHealth;
				BattleUI.Instance.enemyHealthBar.SetHealthImmediate(ratio);
			}
		}
		
		// 전투 시작 시 방어도 아이콘 숨김
		if (BattleUI.Instance != null)
		{
			if (BattleUI.Instance.partyBlockIcons != null)
			{
				foreach (var icon in BattleUI.Instance.partyBlockIcons)
				{
					if (icon != null) icon.SetActive(false);
				}
			}
			
			if (BattleUI.Instance.enemyBlockIcon != null)
			{
				BattleUI.Instance.enemyBlockIcon.SetActive(false);
			}
		}
		
		StartPlayerTurn();
	}
    
    public void StartPlayerTurn()
	{
		if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
			return;
			
		currentState = BattleState.PlayerTurn;
		currentEnergy = maxEnergy;
		
		// 모든 파티원의 방어도 초기화
		foreach (var member in party)
		{
			member.StartTurn();
		}
		
		// 카드 드로우
		if (CardManager.Instance != null)
		{
			CardManager.Instance.StartTurn(); // ← 여기서 드로우
		}
		
		BattleUI.Instance?.UpdateAllUI();
		
		Debug.Log($"--- 플레이어 턴 시작! (에너지: {currentEnergy}) ---");
	}
    
    public void EndPlayerTurn()
    {
        Debug.Log("플레이어 턴 종료!");
    
		// 손패 버리기
		if (CardManager.Instance != null)
		{
			CardManager.Instance.EndTurn();
		}
		
		StartEnemyTurn();
    }
    
    void StartEnemyTurn()
	{
		currentState = BattleState.EnemyTurn;
		
		if (enemy != null)
		{
			enemy.StartTurn();
		}
		
		Debug.Log("--- 적 턴 시작! ---");
		
		// 적이 살아있는 파티원 공격
		if (party.Count > 0 && enemy != null)
		{
			// 살아있는 파티원만 타겟
			List<BattleEntity> aliveMembers = party.FindAll(m => m.currentHealth > 0);
			
			if (aliveMembers.Count > 0)
			{
				BattleEntity target = aliveMembers[Random.Range(0, aliveMembers.Count)];
				
				// ← 적의 결정된 행동 실행!
				enemy.ExecuteAction(target);
			}
		}
		
		BattleUI.Instance?.UpdateAllUI();
		
		Invoke("StartPlayerTurn", 1.5f);
	}
    
    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            Debug.Log($"에너지 {amount} 사용! (남은 에너지: {currentEnergy})");
            BattleUI.Instance?.UpdateEnergyUI();
            return true;
        }
        
        Debug.Log("에너지가 부족합니다!");
        return false;
    }
    
    void CheckPartyWipe()
    {
        // 모든 파티원이 죽었는지 확인
        bool allDead = true;
        foreach (var member in party)
        {
            if (member.currentHealth > 0)
            {
                allDead = false;
                break;
            }
        }
        
        if (allDead)
        {
            currentState = BattleState.Defeat;
            Debug.Log("===== 패배... =====");
        }
    }
    
    public void CheckBattleEnd()
	{
		// 적이 죽었는지 확인
		if (enemy != null && enemy.currentHealth <= 0)
		{
			Debug.Log("===== 승리! =====");
			
			// ← 골드 획득!
			int goldReward = Random.Range(15, 31); // 15~30 골드
			if (GameData.Instance != null)
			{
				GameData.Instance.gold += goldReward;
				Debug.Log($"골드 +{goldReward}! (현재: {GameData.Instance.gold} 골드)");
			}
			
			// 층 진행 (여기서 한 번만!)
			if (RunData.Instance != null && RunData.Instance.isInDungeon)
			{
				RunData.Instance.AdvanceFloor();
				Debug.Log($"층 증가! 현재 층: {RunData.Instance.currentFloor}");
			}
			
			// RewardScene으로 이동
			UnityEngine.SceneManagement.SceneManager.LoadScene("RewardScene");
			return;
		}
		
		// 패배 처리
		bool allDead = true;
		foreach (var member in party)
		{
			if (member.currentHealth > 0)
			{
				allDead = false;
				break;
			}
		}
		
		if (allDead)
		{
			Debug.Log("===== 패배... =====");
			UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
		}
	}
}