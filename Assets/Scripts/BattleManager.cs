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
    
    [Header("Party System")]
    public int partyMaxHealth;      // 파티 전체 최대 체력
    public int partyCurrentHealth;  // 파티 전체 현재 체력
    public int partyCurrentBlock;   // 파티 전체 현재 방어도
    
    [Header("Party Characters")]
    public List<CharacterData> partyCharacters = new List<CharacterData>(); // 파티원 참조
    
    [Header("Party UI")]
    public Transform character1UI;
    public Transform character2UI;
    public Transform character3UI;
    
    [Header("Enemies")]
    public List<EnemyInstance> enemies = new List<EnemyInstance>(); // 적 여러 명
    public Transform enemySpawnParent; // 적 생성 위치
    
    public BattleState currentState;
    public int revivedRequiredCount = 5; // 부활에 필요한 정신집중 카드 사용 횟수
    
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
        partyCharacters.Clear();
        
        // GameData에서 파티원 가져오기
        if (GameData.Instance != null && GameData.Instance.raidParty.Count >= 3)
        {
            // 파티원 복사 (참조)
            partyCharacters.AddRange(GameData.Instance.raidParty);
            
			NormalizeDownStateAtBattleStart();
			
            // 파티 전체 체력 계산
            CalculatePartyHealth();
			
			Debug.Log($"[HP CHECK] 공유HP: {partyCurrentHealth}/{partyMaxHealth}  (GameData: {GameData.Instance?.partyCurrentHealth}/{GameData.Instance?.partyMaxHealth})");
            
            Debug.Log($"파티 구성: {partyCharacters.Count}명");
            Debug.Log($"파티 전체 체력: {partyCurrentHealth}/{partyMaxHealth}");
            
            // 각 캐릭터 정보 출력
            for (int i = 0; i < partyCharacters.Count; i++)
            {
                var character = partyCharacters[i];
                Debug.Log($"  [{i}] {character.characterName}: HP {character.maxHealth} / ATK {character.attackPower} / DEF {character.defensePower} / MP {character.currentMentalPower}/{character.maxMentalPower}");
            }
        }
        else
        {
            Debug.LogError("GameData 또는 raidParty가 없습니다!");
        }
        
        // 적 생성
        SpawnEnemies();
		
		RelicManager.Instance?.OnBattleStart();
        
        // UI 초기화
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
        
        currentState = BattleState.PlayerTurn;
        StartPlayerTurn();
    }
    
    // 파티 전체 체력 계산 + GameData에 저장된 공유 HP 이어받기
	void CalculatePartyHealth()
	{
		partyMaxHealth = 0;

		foreach (var character in partyCharacters)
		{
			if (character == null) continue;

			// 쓰러진 캐릭을 최대체력에서 제외하는 설계면 이 조건 유지
			if (!character.isDefeated)
				partyMaxHealth += character.maxHealth;
		}

		// ✅ GameData에서 공유 HP 이어받기
		if (GameData.Instance != null)
		{
			// max는 지금 계산한 값이 "진짜"니까 GameData도 갱신
			GameData.Instance.partyMaxHealth = partyMaxHealth;

			// current는 GameData 저장값을 우선 사용
			if (GameData.Instance.partyCurrentHealth <= 0)
				GameData.Instance.partyCurrentHealth = partyMaxHealth;

			partyCurrentHealth = Mathf.Clamp(GameData.Instance.partyCurrentHealth, 0, partyMaxHealth);
		}
		else
		{
			// GameData 없으면 어쩔 수 없이 풀피
			partyCurrentHealth = partyMaxHealth;
		}
	}
	
	void SavePartyHPToGameData()
	{
		if (GameData.Instance == null) return;

		GameData.Instance.partyMaxHealth = partyMaxHealth;
		GameData.Instance.partyCurrentHealth = partyCurrentHealth;
	}


    
    // 적 생성
    void SpawnEnemies()
    {
        // 기존 적 전부 제거
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();
        
        if (EnemyDatabase.Instance == null)
        {
            Debug.LogError("EnemyDatabase가 없습니다!");
            return;
        }
        
        // enemySpawnParent 확인
        if (enemySpawnParent == null)
        {
            Debug.LogError("Enemy Spawn Parent가 설정되지 않았습니다!");
            return;
        }
        
        // ← 이 부분 삭제! (자식 제거 안 함)
        // foreach (Transform child in enemySpawnParent)
        // {
        //     Destroy(child.gameObject);
        // }
        
        // 적 1마리만 생성
        EnemyData enemyData = EnemyDatabase.Instance.GetRandomEnemy();
        
        // enemySpawnParent 아래 EnemyPanel 찾기
        Transform enemyPanel = enemySpawnParent.Find("EnemyPanel");
        
        if (enemyPanel == null)
        {
            Debug.LogError("EnemyPanel을 찾을 수 없습니다! enemySpawnParent 아래에 EnemyPanel이 있어야 합니다!");
            return;
        }
        
        // GameObject로 변환
        GameObject enemyPanelObj = enemyPanel.gameObject;
        
        // EnemyPanel에 EnemyInstance 추가 (이미 있으면 재사용)
        EnemyInstance enemyInstance = enemyPanelObj.GetComponent<EnemyInstance>();
        if (enemyInstance == null)
        {
            enemyInstance = enemyPanelObj.AddComponent<EnemyInstance>();
        }
        
        enemyInstance.Initialize(enemyData);
        enemies.Add(enemyInstance);
        
        Debug.Log($"적 생성 완료: {enemyData.enemyName} (체력: {enemyData.maxHealth})");
    }
    
    public void StartPlayerTurn()
    {
        if (currentState == BattleState.Victory || currentState == BattleState.Defeat)
            return;
        
        currentState = BattleState.PlayerTurn;
        currentEnergy = maxEnergy;
        
        // 파티 방어도 초기화
        partyCurrentBlock = 0;
        
        // 카드 드로우
        if (CardManager.Instance != null)
        {
            CardManager.Instance.StartTurn();
        }
		
		RelicManager.Instance?.OnPlayerTurnStart();
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
        
        Debug.Log($"--- 플레이어 턴 시작! (에너지: {currentEnergy}) ---");
    }
    
    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn)
            return;
        
        Debug.Log("--- 플레이어 턴 종료 ---");
        
        // 손패의 카드 버리기
        if (CardManager.Instance != null)
        {
            CardManager.Instance.EndTurn();
        }
        
        currentState = BattleState.EnemyTurn;
        
        // 적 턴 시작 (약간 딜레이)
        Invoke("StartEnemyTurn", 0.5f);
    }
    
    void StartEnemyTurn()
    {
        Debug.Log("--- 적 턴 시작! ---");
        
        // 모든 적의 방어도 초기화
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                enemy.currentDefense = 0;
            }
        }
        
        // 모든 적이 행동
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                EnemyAction action = enemy.PerformAction();
                
                if (action != null)
                {
                    ExecuteEnemyAction(enemy, action);
                }
            }
        }
        
        // 전투 종료 체크
        if (CheckVictory())
        {
            Victory();
            return;
        }
        
        if (CheckDefeat())
        {
            Defeat();
            return;
        }
        
        // 플레이어 턴으로
        Invoke("StartPlayerTurn", 1f);
    }
    
    // 적 행동 실행
    void ExecuteEnemyAction(EnemyInstance enemy, EnemyAction action)
    {
        switch (action.type)
        {
            case EnemyAction.ActionType.Attack:
                // 랜덤 타겟 선택
                CharacterData target = GetRandomAliveCharacter();
                
                if (target != null)
                {
                    DamageParty(action.value, target, action.mentalAttackValue);
                }
                break;
                
            case EnemyAction.ActionType.Defend:
                enemy.GainDefense(action.value);
                break;
        }
    }
    
    // 파티가 데미지를 받음
    public void DamageParty(int damage, CharacterData targetCharacter, int mentalDamage)
    {
        // 1. 파티 방어도로 데미지 감소
        int actualDamage = Mathf.Max(0, damage - partyCurrentBlock);
        
        if (actualDamage > 0)
        {
            // 파티 방어도 차감
            partyCurrentBlock = Mathf.Max(0, partyCurrentBlock - damage);
            
            // 파티 체력 차감
            partyCurrentHealth -= actualDamage;
            
            // 최대 체력 초과 방지
            if (partyCurrentHealth > partyMaxHealth)
            {
                partyCurrentHealth = partyMaxHealth;
            }
            
            Debug.Log($"파티가 {actualDamage} 데미지를 받음! ({partyCurrentHealth}/{partyMaxHealth})");
        }
        else
        {
            // 방어도만 차감
            partyCurrentBlock -= damage;
            Debug.Log($"파티 방어도로 막음! (남은 방어도: {partyCurrentBlock})");
        }
        
        // 2. 타겟 캐릭터 정신력 감소
        if (targetCharacter != null && mentalDamage > 0)
        {
            targetCharacter.TakeMentalDamage(mentalDamage);
            Debug.Log($"{targetCharacter.characterName}의 정신력 {mentalDamage} 감소! ({targetCharacter.currentMentalPower}/{targetCharacter.maxMentalPower})");
            
            // 쓰러짐 체크
            if (targetCharacter.isDefeated)
            {
                OnCharacterDefeated(targetCharacter);
            }
        }
        
        // UI 업데이트
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
		
		SavePartyHPToGameData();
    }
    
    // 랜덤 생존 캐릭터 선택
    CharacterData GetRandomAliveCharacter()
    {
        List<CharacterData> aliveCharacters = new List<CharacterData>();
        
        foreach (var character in partyCharacters)
        {
            if (!character.isDefeated)
            {
                aliveCharacters.Add(character);
            }
        }
        
        if (aliveCharacters.Count > 0)
        {
            return aliveCharacters[Random.Range(0, aliveCharacters.Count)];
        }
        
        return null;
    }
    
    // 캐릭터 쓰러짐 처리
    void OnCharacterDefeated(CharacterData character)
    {
        Debug.Log($"{character.characterName} 쓰러짐!");
        
        // 파티 최대 체력에서 해당 캐릭터 체력 제거
        partyMaxHealth -= character.maxHealth;
		
		SavePartyHPToGameData();
        
        // 현재 체력이 최대 체력보다 크면 조정
        if (partyCurrentHealth > partyMaxHealth)
        {
            partyCurrentHealth = partyMaxHealth;
        }
        
        Debug.Log($"파티 최대 체력 감소: {partyMaxHealth} (현재: {partyCurrentHealth})");
        
        // 모든 카드를 정신집중으로 변경
        ConvertCardsToRevive(character);
        
        // UI 업데이트
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
    
    // 카드를 정신집중으로 변경
    void ConvertCardsToRevive(CharacterData character)
    {
        if (CardManager.Instance == null) return;
        
        CardManager.Instance.ConvertCharacterCardsToRevive(character);
    }
    
    // ← 정신집중 카드 사용 (부활 카운트)
    public void UseReviveCard(CharacterData character)
    {
        if (character == null) return;

		bool isDown = character.isDefeated || character.currentMentalPower <= 0;
		if (!isDown) return;

		// 상태가 꼬여있으면 여기서 정규화
		character.isDefeated = true;

        
        character.reviveCardUseCount++;
        
        Debug.Log($"{character.characterName} 정신집중 사용! ({character.reviveCardUseCount}/{revivedRequiredCount})");
        
        // 필요 횟수 달성 시 부활
        if (character.reviveCardUseCount >= revivedRequiredCount)
        {
            ReviveCharacter(character);
        }
        
        // UI 업데이트
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
    
    // ← 캐릭터 부활
    void ReviveCharacter(CharacterData character)
    {
        Debug.Log($"===== {character.characterName} 부활! =====");
        
        // 부활 처리
        character.Revive();
        
        // 파티 최대 체력 복구
        partyMaxHealth += character.maxHealth;
        
        // 체력 1/3 회복
        int healthRestore = character.maxHealth / 3;
        partyCurrentHealth += healthRestore;
        
        // 최대 체력 초과 방지
        if (partyCurrentHealth > partyMaxHealth)
        {
            partyCurrentHealth = partyMaxHealth;
        }
        
        Debug.Log($"파티 체력: {partyCurrentHealth}/{partyMaxHealth}");
        
        // 카드 복구
        if (CardManager.Instance != null)
        {
            CardManager.Instance.RestoreCharacterCards(character);
        }
        
        // UI 업데이트
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
    
    // 파티가 방어도 획득
    public void PartyGainBlock(int amount)
    {
        partyCurrentBlock += amount;
        Debug.Log($"파티가 {amount} 방어도 획득! (현재: {partyCurrentBlock})");
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
    }
    
    // 파티 체력 회복
    public void PartyHeal(int amount)
    {
        partyCurrentHealth += amount;
        
        if (partyCurrentHealth > partyMaxHealth)
        {
            partyCurrentHealth = partyMaxHealth;
        }
        
        Debug.Log($"파티 체력 {amount} 회복! ({partyCurrentHealth}/{partyMaxHealth})");
        
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.UpdateAllUI();
        }
		
		SavePartyHPToGameData(); // ✅ 추가
    }
    
    // 적에게 데미지
    public void DamageEnemy(EnemyInstance target, int damage)
    {
        if (target == null) return;
        
        target.TakeDamage(damage);
        
        // ← 데미지 팝업 표시
        if (DamagePopupManager.Instance != null)
        {
            Vector3 popupPosition = target.transform.position;
            DamagePopupManager.Instance.ShowDamage(damage, popupPosition);
            Debug.Log($"데미지 팝업 표시: {damage}");
        }
        else
        {
            Debug.LogWarning("DamagePopupManager가 없습니다!");
        }
        
        // 적 사망 체크
        if (target.currentHealth <= 0)
        {
            enemies.Remove(target);
            Destroy(target.gameObject, 0.5f);
            
            if (CheckVictory())
            {
                Victory();
            }
        }
    }
    
    // 승리 체크
    bool CheckVictory()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                return false;
            }
        }
        return true;
    }
    
    // 패배 체크
    bool CheckDefeat()
    {
        // 파티 체력이 0이거나, 모든 캐릭터가 쓰러짐
        if (partyCurrentHealth <= 0)
        {
            return true;
        }
        
        // 모든 캐릭터가 쓰러졌는지 체크
        int defeatedCount = 0;
        foreach (var character in partyCharacters)
        {
            if (character.isDefeated)
            {
                defeatedCount++;
            }
        }
        
        return defeatedCount >= partyCharacters.Count;
    }
    
    void Victory()
    {
        currentState = BattleState.Victory;
        Debug.Log("===== 승리! =====");
        
        // GameData에 현재 상태 저장
        if (GameData.Instance != null)
        {
            for (int i = 0; i < partyCharacters.Count; i++)
            {
                GameData.Instance.raidParty[i].currentHealth = partyCharacters[i].currentHealth;
                GameData.Instance.raidParty[i].currentMentalPower = partyCharacters[i].currentMentalPower;
                GameData.Instance.raidParty[i].isDefeated = partyCharacters[i].isDefeated;
            }
        }
        
        // ← RunData 업데이트 (중요!)
        if (RunData.Instance != null)
        {
            RunData.Instance.battlesWon++;
            Debug.Log($"전투 승리! 총 {RunData.Instance.battlesWon}승");
        }
        
		SavePartyHPToGameData();
		
        // 보상 화면으로
        Invoke("LoadRewardScene", 2f);
    }
    
    void Defeat()
    {
        currentState = BattleState.Defeat;
        Debug.Log("===== 패배... =====");
        
        // 게임 오버 처리 (약간 딜레이)
        Invoke("LoadGameOverScene", 2f);
    }
    
    void LoadRewardScene()
    {
        // 씬 존재 여부 확인
        string sceneName = "RewardScene";
        bool sceneExists = false;
        
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains(sceneName))
            {
                sceneExists = true;
                break;
            }
        }
        
        if (sceneExists)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("RewardScene이 없습니다! DungeonMapScene으로 이동");
			// RewardScene이 없으면 여기서 다음 층으로 진행 처리
			if (RunData.Instance != null && RunData.Instance.isInDungeon)
			{
				RunData.Instance.AdvanceFloor();
			}
            UnityEngine.SceneManagement.SceneManager.LoadScene("DungeonMapScene");
        }
    }
    
    void LoadGameOverScene()
    {
        // 게임 오버 씬이 있으면 로드, 없으면 던전 맵으로
        SceneManager.LoadScene("DungeonMapScene");
    }
	
	void NormalizeDownStateAtBattleStart()
	{
		for (int i = 0; i < partyCharacters.Count; i++)
		{
			var c = partyCharacters[i];
			if (c == null) continue;

			// 정신력 0이면 DOWN 상태로 고정
			if (c.currentMentalPower <= 0)
			{
				c.isDefeated = true;

				if (c.reviveCardUseCount < 0)
					c.reviveCardUseCount = 0;
			}
		}
	}

}