using UnityEngine;

[System.Serializable]
public enum CharacterType
{
    Warrior,    // 전사
    Mage,       // 마법사
    Rogue       // 도적
}

public class Card : MonoBehaviour
{
    [Header("카드 데이터")]
    public CardData cardData;
    
    [Header("배율 기반 효과")]
    public float damageMultiplier;
    public float defenseMultiplier;
    public int mentalRestoreAmount;
    
    // 기존 속성들
    public string cardName
    {
        get => cardData?.cardName ?? "";
        set { if (cardData != null) cardData.cardName = value; }
    }
    
    public CardType cardType
    {
        get => cardData?.cardType ?? CardType.Attack;
        set { if (cardData != null) cardData.cardType = value; }
    }
    
    public int cost
    {
        get => cardData?.cost ?? 0;
        set { if (cardData != null) cardData.cost = value; }
    }
    
    public int value
    {
        get => cardData?.value ?? 0;
        set { if (cardData != null) cardData.value = value; }
    }
    
    public int characterIndex
    {
        get => cardData?.characterIndex ?? 0;
        set { if (cardData != null) cardData.characterIndex = value; }
    }
    
    public CardEffect specialEffect
    {
        get => cardData?.specialEffect ?? CardEffect.None;
        set { if (cardData != null) cardData.specialEffect = value; }
    }
    
    // ← effectValue 추가!
    public int effectValue
    {
        get => cardData?.effectValue ?? 0;
        set { if (cardData != null) cardData.effectValue = value; }
    }
    
    public string description
    {
        get => cardData?.description ?? "";
        set { if (cardData != null) cardData.description = value; }
    }
    
    public Sprite cardImage
    {
        get => null;
        set { }
    }
    
    public void SetFromCardData(CardData data)
    {
        cardData = data;
        
        switch (data.cardType)
        {
            case CardType.Attack:
                damageMultiplier = 0.9f;
                break;
            case CardType.Skill:
                defenseMultiplier = 1.3f;
                break;
        }
    }
    
    public int CalculateDamage(CharacterData character)
    {
        if (damageMultiplier > 0)
        {
            return Mathf.RoundToInt(character.attackPower * damageMultiplier);
        }
        return cardData?.value ?? 0;
    }
    
    public int CalculateDefense(CharacterData character)
    {
        if (defenseMultiplier > 0)
        {
            return Mathf.RoundToInt(character.defensePower * defenseMultiplier);
        }
        return cardData?.value ?? 0;
    }
    
    public virtual void PlayCard()
    {
        Debug.Log($"{cardName} 카드 사용! 코스트: {cost}");
    }
}