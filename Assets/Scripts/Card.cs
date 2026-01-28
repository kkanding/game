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
    
    // SetFromCardData 메서드
    public void SetFromCardData(CardData data)
    {
        cardData = data;
        
        // 캐릭터 인덱스로 배율 설정
        if (GameData.Instance != null && data.characterIndex < GameData.Instance.raidParty.Count)
        {
            CharacterData character = GameData.Instance.raidParty[data.characterIndex];
            
            // 카드 타입에 따라 배율 설정
            switch (data.cardType)
            {
                case CardType.Attack:
                    // 전사
                    if (character.characterName == "전사")
                    {
                        if (data.cardName.Contains("타격"))
                            damageMultiplier = 0.9f;
                        else if (data.cardName.Contains("강타"))
                            damageMultiplier = 1.8f;
                        else if (data.cardName.Contains("광전사"))
                            damageMultiplier = 1.2f;
                    }
                    // 마법사
                    else if (character.characterName == "마법사")
                    {
                        if (data.cardName.Contains("화염구"))
                            damageMultiplier = 0.85f;
                        else if (data.cardName.Contains("번개"))
                            damageMultiplier = 0.8f;
                        else if (data.cardName.Contains("얼음 창"))
                            damageMultiplier = 1.5f;
                    }
                    // 도적
                    else if (character.characterName == "도적")
                    {
                        if (data.cardName.Contains("암습"))
                            damageMultiplier = 0.9f;
                        else if (data.cardName.Contains("독칼"))
                            damageMultiplier = 0.6f;
                        else if (data.cardName.Contains("급소"))
                            damageMultiplier = 1.8f;
                    }
                    break;
                    
                case CardType.Skill:
                    // 전사
                    if (character.characterName == "전사")
                    {
                        if (data.cardName.Contains("방어"))
                            defenseMultiplier = 1.3f;
                        else if (data.cardName.Contains("철벽"))
                            defenseMultiplier = 2.6f;
                    }
                    // 마법사
                    else if (character.characterName == "마법사")
                    {
                        if (data.cardName.Contains("방벽"))
                            defenseMultiplier = 1.5f;
                        else if (data.cardName.Contains("마나 실드"))
                            defenseMultiplier = 1.4f;
                        else if (data.cardName.Contains("명상"))
                            mentalRestoreAmount = 25;
                    }
                    // 도적
                    else if (character.characterName == "도적")
                    {
                        if (data.cardName.Contains("회피"))
                            defenseMultiplier = 1.4f;
                        else if (data.cardName.Contains("연막"))
                            defenseMultiplier = 1.8f;
                        else if (data.cardName.Contains("그림자"))
                            defenseMultiplier = 1.4f;
                    }
                    break;
            }
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