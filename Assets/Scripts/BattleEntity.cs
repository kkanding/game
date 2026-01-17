using UnityEngine;
using System;

public class BattleEntity : MonoBehaviour
{
    [Header("Stats")]
    public string entityName;
    public int maxHealth;
    public int currentHealth;
    public int block; // 방어도
	
	// ← 적 의도 시스템
    public EnemyData enemyData;
    public EnemyAction nextAction;
    public int actionIndex = 0;
    
    public event Action<int> OnHealthChanged;
    public event Action<int> OnBlockChanged;
    public event Action OnDeath;
    
    void Awake()  // Start 대신 Awake 사용
    {
        currentHealth = maxHealth;
    }
    
    // 데미지 받기
    public void TakeDamage(int amount)
{
    int actualDamage = Mathf.Max(0, amount - block);
    
    if (actualDamage > 0)
    {
        // 방어도를 먼저 깎음
        if (block > 0)
        {
            block = Mathf.Max(0, block - amount);
        }
        
        // 남은 데미지를 체력에서 차감
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"{entityName}이(가) {actualDamage}의 피해를 입음! (남은 체력: {currentHealth})");
        
        // 데미지 팝업 표시
        if (DamagePopupManager.Instance != null)
        {
            // UI 오브젝트의 위치 사용 (RectTransform)
            Vector3 popupPosition = transform.position;
            
            // Canvas 내부 오브젝트라면 월드 좌표로 변환
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Canvas 좌표를 그대로 사용
                popupPosition = rectTransform.position;
            }
            
            DamagePopupManager.Instance.ShowDamage(actualDamage, popupPosition);
        }
    }
    else
    {
        // 방어도만 깎임
        block -= amount;
        Debug.Log($"{entityName}의 방어도가 {amount} 감소! (남은 방어도: {block})");
    }
    
    BattleUI.Instance?.UpdatePartyUI();
    BattleUI.Instance?.UpdateEnemyUI();
}
    
    // 방어도 얻기
    public void GainBlock(int amount)
    {
        block += amount;
        OnBlockChanged?.Invoke(block);
        Debug.Log($"{entityName}이(가) {amount} 방어도를 얻었습니다! (현재 방어도: {block})");
    }
    
    // 체력 회복
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"{entityName}이(가) {amount} 체력을 회복했습니다! (현재 체력: {currentHealth})");
    }
    
    // 사망 처리
    void Die()
    {
        Debug.Log($"{entityName}이(가) 사망했습니다!");
        OnDeath?.Invoke();
    }
    
    // 턴 시작 시 방어도 초기화
    public void StartTurn()
    {
        block = 0;
        OnBlockChanged?.Invoke(block);
    }
	
	// 적 의도 결정
	public void DecideNextAction()
	{
		if (enemyData == null || enemyData.actionPattern.Count == 0)
		{
			// 기본 공격
			nextAction = new EnemyAction(EnemyAction.ActionType.Attack, UnityEngine.Random.Range(5, 11), "공격");
			return;
		}
		
		// 패턴에서 순서대로 행동 선택
		nextAction = enemyData.actionPattern[actionIndex];
		actionIndex = (actionIndex + 1) % enemyData.actionPattern.Count;
		
		Debug.Log($"{entityName}의 다음 행동: {nextAction.description} ({nextAction.value})");
	}

	// 결정된 행동 실행
	public void ExecuteAction(BattleEntity target)
	{
		if (nextAction == null)
		{
			DecideNextAction();
		}
		
		switch (nextAction.type)
		{
			case EnemyAction.ActionType.Attack:
				Debug.Log($"{entityName}이(가) {target.entityName}에게 {nextAction.value} 데미지 공격!");
				target.TakeDamage(nextAction.value);
				break;
				
			case EnemyAction.ActionType.Defend:
				Debug.Log($"{entityName}이(가) {nextAction.value} 방어도 획득!");
				GainBlock(nextAction.value);
				break;
				
			case EnemyAction.ActionType.Buff:
				Debug.Log($"{entityName}이(가) 버프 사용!");
				// 버프 구현 (나중에)
				break;
				
			case EnemyAction.ActionType.Debuff:
				Debug.Log($"{entityName}이(가) 디버프 사용!");
				// 디버프 구현 (나중에)
				break;
		}
		
		// 다음 행동 결정
		DecideNextAction();
	}
}