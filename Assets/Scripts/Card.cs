using UnityEngine;

[System.Serializable]
public enum CardType
{
    Attack,
    Skill,
    Power
}

// ← 특수 효과 enum 추가!
[System.Serializable]
public enum CardEffect
{
    None,           // 효과 없음
    DrawCard,       // 카드 드로우
    GainEnergy,     // 에너지 획득
    SelfDamage,     // 자신에게 데미지
    Poison,         // 독
    Weak,           // 약화
    Vulnerable,     // 취약
    Strength        // 힘
}

public class Card : MonoBehaviour
{
    [Header("Card Data")]
    public string cardName;
    public CardType cardType;
    public int cost;
    public int value;
    
    [TextArea]
    public string description;
    
    [Header("Visual")]
    public Sprite cardImage;
    
    [Header("Character Info")]
    public int characterIndex = 0;
    
    [Header("Special Effects")]
    public CardEffect specialEffect = CardEffect.None;
    public int effectValue = 0; // 특수 효과 값
    
	//CardData로부터 데이터 설정
    public void SetFromCardData(CardData data)
    {
        cardName = data.cardName;
        cardType = data.cardType;
        cost = data.cost;
        value = data.value;
        description = data.description;
        characterIndex = data.characterIndex;
        specialEffect = data.specialEffect; // ← 특수 효과 복사!
        effectValue = data.effectValue;
    }
	
    public virtual void PlayCard()
    {
        if (!BattleManager.Instance.UseEnergy(cost))
            return;
        
        Debug.Log($"{cardName} 카드 사용!");
        
        // 기본 효과
        switch (cardType)
        {
            case CardType.Attack:
                if (BattleManager.Instance.enemy != null)
                {
                    BattleManager.Instance.enemy.TakeDamage(value);
                }
                break;
                
            case CardType.Skill:
                if (BattleManager.Instance.party.Count > characterIndex)
                {
                    BattleManager.Instance.party[characterIndex].GainBlock(value);
                }
                break;
        }
        
        // 특수 효과
        ApplySpecialEffect();
        
        BattleManager.Instance.CheckBattleEnd();
        
        // CardManager를 통해 카드 사용 처리
        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        if (cardDisplay != null && CardManager.Instance != null)
        {
            CardManager.Instance.PlayCard(cardDisplay);
        }
    }
    
    void ApplySpecialEffect()
    {
        switch (specialEffect)
        {
            case CardEffect.DrawCard:
                // 카드 드로우
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.DrawCards(effectValue);
                    Debug.Log($"{effectValue}장 드로우!");
                }
                break;
                
            case CardEffect.GainEnergy:
                // 에너지 획득
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.currentEnergy += effectValue;
                    BattleUI.Instance?.UpdateEnergyUI();
                    Debug.Log($"에너지 +{effectValue}!");
                }
                break;
                
            case CardEffect.SelfDamage:
                // 자신에게 데미지
                if (BattleManager.Instance.party.Count > characterIndex)
                {
                    BattleManager.Instance.party[characterIndex].TakeDamage(effectValue);
                    Debug.Log($"자신에게 {effectValue} 데미지!");
                }
                break;
                
            case CardEffect.Poison:
                Debug.Log($"독 효과 (미구현): {effectValue}턴");
                // TODO: 독 시스템 구현
                break;
                
            case CardEffect.Weak:
                Debug.Log($"약화 효과 (미구현)");
                // TODO: 약화 시스템 구현
                break;
                
            case CardEffect.Vulnerable:
                Debug.Log($"취약 효과 (미구현)");
                // TODO: 취약 시스템 구현
                break;
                
            case CardEffect.Strength:
                Debug.Log($"힘 효과 (미구현): +{effectValue}");
                // TODO: 힘 시스템 구현
                break;
        }
    }
}