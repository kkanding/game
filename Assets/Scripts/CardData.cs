using UnityEngine;

[System.Serializable]
public class CardData
{
    public string cardName;
    public CardType cardType;
    public int cost;
    public int value;
    public string description;
    public int characterIndex;
    
    public CardEffect specialEffect = CardEffect.None;
    public int effectValue = 0;
    
    // ← 업그레이드 시스템 추가!
    public bool isUpgraded = false;
    public int upgradedValue = 0; // 업그레이드 시 값
    public int upgradedCost = 0; // 업그레이드 시 비용
    
    public CardData(string name, CardType type, int cost, int value, string description, int charIndex)
    {
        cardName = name;
        cardType = type;
        this.cost = cost;
        this.value = value;
        this.description = description;
        characterIndex = charIndex;
        
        // 기본 업그레이드 값 설정
        upgradedCost = cost;
        upgradedValue = value;
    }
    
    // 카드 업그레이드
    public CardData Upgrade()
    {
        if (isUpgraded)
        {
            Debug.Log($"{cardName}는 이미 업그레이드되었습니다!");
            return this;
        }
        
        // 복사본 생성
        CardData upgraded = new CardData(cardName, cardType, cost, value, description, characterIndex);
        upgraded.specialEffect = specialEffect;
        upgraded.effectValue = effectValue;
        upgraded.isUpgraded = true;
        
        // 업그레이드 적용
        switch (cardType)
        {
            case CardType.Attack:
                // 공격 카드: 데미지 +50%
                upgraded.value = (int)(value * 1.5f);
                upgraded.upgradedValue = upgraded.value;
                break;
                
            case CardType.Skill:
                // 스킬 카드: 효과 +50% 또는 비용 -1
                if (cost > 0 && value < 5)
                {
                    // 비용 감소
                    upgraded.cost = cost - 1;
                    upgraded.upgradedCost = upgraded.cost;
                }
                else
                {
                    // 효과 증가
                    upgraded.value = (int)(value * 1.5f);
                    upgraded.upgradedValue = upgraded.value;
                }
                
                // 특수 효과도 증가
                if (effectValue > 0)
                {
                    upgraded.effectValue = effectValue + 1;
                }
                break;
                
            case CardType.Power:
                // 파워 카드: 효과 +1 또는 비용 -1
                if (cost > 0)
                {
                    upgraded.cost = cost - 1;
                    upgraded.upgradedCost = upgraded.cost;
                }
                else
                {
                    upgraded.value = value + 1;
                    upgraded.upgradedValue = upgraded.value;
                }
                break;
        }
        
        // 이름에 + 추가
        upgraded.cardName = cardName + "+";
        
        // 설명 업데이트
        upgraded.description = UpdateDescription(upgraded);
        
        Debug.Log($"{cardName} → {upgraded.cardName} 업그레이드 완료!");
        
        return upgraded;
    }
    
    // 업그레이드된 설명 생성
    string UpdateDescription(CardData card)
    {
        string desc = "";
        
        switch (card.cardType)
        {
            case CardType.Attack:
                desc = $"적에게 {card.value}의 피해를 준다.";
                break;
                
            case CardType.Skill:
                if (card.value > 0)
                {
                    desc = $"{card.value}의 방어도를 얻는다.";
                }
                
                if (card.specialEffect == CardEffect.DrawCard)
                {
                    desc += $" 카드 {card.effectValue}장 드로우.";
                }
                else if (card.specialEffect == CardEffect.GainEnergy)
                {
                    desc += $" 에너지 +{card.effectValue}";
                }
                break;
                
            case CardType.Power:
                desc = description; // 기존 설명 유지
                break;
        }
        
        return desc;
    }
}