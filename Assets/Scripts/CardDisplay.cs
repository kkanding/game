using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler // ← IPointerClickHandler 추가
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image cardImage;
    public Image backgroundImage;
	
	[Header("Upgrade")] // ← 추가!
	public GameObject upgradeIndicator; // + 표시
    
    private Card cardData;
    private CardAnimator animator;
	public bool disableClick = false;
    
    void Awake()
    {
        animator = GetComponent<CardAnimator>();
    }
    
    public void SetupCard(Card card)
	{
		if (card == null) return;
		
		cardData = card;
		
		// 카드 이름
		if (nameText != null)
		{
			nameText.text = card.cardName;
		}
		
		// 코스트
		if (costText != null)
		{
			costText.text = card.cost.ToString();
		}
		
		// 설명
		if (descriptionText != null)
		{
			descriptionText.text = card.description;
		}
		
		// ← 캐릭터별 배경 색상!
		if (backgroundImage != null && GameData.Instance != null)
		{
			if (card.characterIndex >= 0 && card.characterIndex < GameData.Instance.raidParty.Count)
			{
				CharacterData character = GameData.Instance.raidParty[card.characterIndex];
				
				switch (character.characterName)
				{
					case "전사":
						backgroundImage.color = new Color(1f, 0.3f, 0.3f); // 빨강
						break;
					case "마법사":
						backgroundImage.color = new Color(0.3f, 0.5f, 1f); // 파랑
						break;
					case "도적":
						backgroundImage.color = new Color(0.3f, 0.8f, 0.3f); // 초록
						break;
					default:
						backgroundImage.color = Color.white;
						break;
				}
			}
		}
		
		// 업그레이드 표시
		if (upgradeIndicator != null)
		{
			upgradeIndicator.SetActive(card.cardName.EndsWith("+"));
		}
	}
    
    void UpdateUI()
    {
        if (cardData == null)
        {
            Debug.LogError("UpdateUI: cardData가 null!");
            return;
        }
        
        if (nameText != null)
            nameText.text = cardData.cardName;
        
        if (costText != null)
            costText.text = cardData.cost.ToString();
        
        if (descriptionText != null)
            descriptionText.text = cardData.description;
        
        if (cardData.cardImage != null && cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
        }
        
        if (backgroundImage != null)
        {
            // 캐릭터별 색상
            Color characterColor = GetCharacterColor(cardData.characterIndex);
            
            // 카드 타입에 따라 밝기 조정
            switch (cardData.cardType)
            {
                case CardType.Attack:
                    backgroundImage.color = characterColor;
                    break;
                case CardType.Skill:
                    backgroundImage.color = characterColor * 0.8f;
                    break;
                case CardType.Power:
                    backgroundImage.color = characterColor * 1.2f;
                    break;
            }
        }
    }
    
    Color GetCharacterColor(int characterIndex)
    {
        switch (characterIndex)
        {
            case 0: // 전사
                return new Color(1f, 0.3f, 0.3f);
            case 1: // 마법사
                return new Color(0.3f, 0.3f, 1f);
            case 2: // 도적
                return new Color(0.3f, 1f, 0.3f);
            default:
                return Color.white;
        }
    }
    
    // 마우스 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.OnHoverEnter();
        }
    }
    
    // 마우스 내렸을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.OnHoverExit();
        }
    }
    
    // ← 카드 클릭 이벤트 추가!
    public void OnPointerClick(PointerEventData eventData)
	{
		// ← 비활성화 플래그 확인
		if (disableClick)
		{
			return;
		}
		
		// BattleManager가 있을 때만 실행 (전투 중)
		if (BattleManager.Instance == null)
		{
			return;
		}
		
		if (cardData == null)
		{
			Debug.LogError("cardData가 null입니다!");
			return;
		}
		
		Debug.Log($"카드 클릭됨! {cardData.cardName}");
		
		// 카드 사용
		cardData.PlayCard();
	}
}