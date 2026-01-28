using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI goldText;
    public Transform shopItemsPanel;
    public GameObject shopItemPrefab;
    public Button leaveButton;
    
    [Header("Shop Settings")]
    public int cardPrice = 50;
    public int removeCardPrice = 75;
    public int healPrice = 40;
	public int relicPrice = 120;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // 나가기 버튼
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(LeaveShop);
        }
        
        // 상점 아이템 생성
        GenerateShopItems();
        
        // 골드 UI 업데이트
        UpdateGoldUI();
    }
    
    void GenerateShopItems()
    {
        if (shopItemPrefab == null || shopItemsPanel == null)
        {
            Debug.LogError("ShopItemPrefab 또는 ShopItemsPanel이 없습니다!");
            return;
        }
        
        // 기존 아이템 제거
        foreach (Transform child in shopItemsPanel)
        {
            Destroy(child.gameObject);
        }
        
        // 1. 랜덤 카드 3장
        for (int i = 0; i < 3; i++)
        {
            CreateCardItem();
        }
        
        // 2. 카드 제거
        CreateRemoveCardItem();
		
		// 3. 유물 구매 (추가)
		CreateRelicItem();
        
        // 4. 체력 회복
        CreateHealItem();
    }
    
    // 카드 구매 아이템
    void CreateCardItem()
    {
        GameObject itemObj = Instantiate(shopItemPrefab, shopItemsPanel);
        
        // 랜덤 카드 선택
        CardData randomCard = GetRandomCardForSale();
        
        if (randomCard == null) return;
        
        // UI 설정
        TextMeshProUGUI nameText = itemObj.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
        TextMeshProUGUI buttonText = buyButton?.GetComponentInChildren<TextMeshProUGUI>();
        
        if (nameText != null) nameText.text = randomCard.cardName;
        if (descText != null) descText.text = randomCard.description;
        if (buttonText != null) buttonText.text = $"구매 ({cardPrice} 골드)";
        
        // 구매 버튼
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => BuyCard(randomCard, buyButton));
        }
        
        // 골드 부족하면 버튼 비활성화
        if (GameData.Instance != null && GameData.Instance.gold < cardPrice)
        {
            if (buyButton != null) buyButton.interactable = false;
        }
    }
    
    // 카드 제거 아이템
    void CreateRemoveCardItem()
    {
        GameObject itemObj = Instantiate(shopItemPrefab, shopItemsPanel);
        
        // UI 설정
        TextMeshProUGUI nameText = itemObj.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
        TextMeshProUGUI buttonText = buyButton?.GetComponentInChildren<TextMeshProUGUI>();
        
        if (nameText != null) nameText.text = "카드 제거";
        if (descText != null) descText.text = "덱에서 카드 1장을 영구히 제거합니다.";
        if (buttonText != null) buttonText.text = $"구매 ({removeCardPrice} 골드)";
        
        // 구매 버튼
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => BuyRemoveCard(buyButton));
        }
        
        // 골드 부족하면 버튼 비활성화
        if (GameData.Instance != null && GameData.Instance.gold < removeCardPrice)
        {
            if (buyButton != null) buyButton.interactable = false;
        }
    }
    
    // 체력 회복 아이템
    void CreateHealItem()
    {
        GameObject itemObj = Instantiate(shopItemPrefab, shopItemsPanel);
        
        // UI 설정
        TextMeshProUGUI nameText = itemObj.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
        TextMeshProUGUI buttonText = buyButton?.GetComponentInChildren<TextMeshProUGUI>();
        
        if (nameText != null) nameText.text = "회복 물약";
        if (descText != null) descText.text = "파티 전체의 체력을 25% 회복합니다.";
        if (buttonText != null) buttonText.text = $"구매 ({healPrice} 골드)";
        
        // 구매 버튼
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => BuyHeal(buyButton));
        }
        
        // 골드 부족하면 버튼 비활성화
        if (GameData.Instance != null && GameData.Instance.gold < healPrice)
        {
            if (buyButton != null) buyButton.interactable = false;
        }
    }
	
	// ===== 유물 구매 아이템 =====
	void CreateRelicItem()
	{
		// RelicManager 없으면 상점에 유물 안 뜸 (안전장치)
		if (RelicManager.Instance == null) return;

		RelicDefinition relic = RelicManager.Instance.GetRandomRelicNotOwned();
		if (relic == null) return; // 전부 보유하면 안 뜸

		GameObject itemObj = Instantiate(shopItemPrefab, shopItemsPanel);

		// UI 설정 (기존 카드/회복 아이템과 동일한 프리팹 구조 사용)
		TextMeshProUGUI nameText = itemObj.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
		Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
		TextMeshProUGUI buttonText = buyButton != null ? buyButton.GetComponentInChildren<TextMeshProUGUI>() : null;

		if (nameText != null) nameText.text = $"유물: {relic.name}";
		if (descText != null) descText.text = relic.desc;
		if (buttonText != null) buttonText.text = $"구매 ({relicPrice} 골드)";

		if (buyButton != null)
		{
			buyButton.onClick.AddListener(() => BuyRelic(relic.id, buyButton));
		}

		// 골드 부족하면 비활성
		if (GameData.Instance != null && GameData.Instance.gold < relicPrice)
		{
			if (buyButton != null) buyButton.interactable = false;
		}
	}

	void BuyRelic(string relicId, Button buyButton)
	{
		if (GameData.Instance == null || RelicManager.Instance == null) return;
		if (GameData.Instance.gold < relicPrice) return;

		bool added = RelicManager.Instance.AddRelic(relicId);
		if (!added) return;

		GameData.Instance.gold -= relicPrice;

		UpdateGoldUI();

		if (buyButton != null) buyButton.interactable = false;
	}

    
    // 랜덤 카드 선택
    CardData GetRandomCardForSale()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0)
            return null;
        
        // 랜덤 캐릭터 선택
        int characterIndex = Random.Range(0, GameData.Instance.raidParty.Count);
        CharacterData character = GameData.Instance.raidParty[characterIndex];
        
        // 해당 캐릭터의 모든 카드
        List<string> availableCards = GetAllCardsForCharacter(character.characterName);
        
        if (availableCards.Count == 0) return null;
        
        string randomCardName = availableCards[Random.Range(0, availableCards.Count)];
        
        return CreateCardData(randomCardName, characterIndex);
    }
    
    // 캐릭터별 카드 목록 (RewardManager와 동일)
    List<string> GetAllCardsForCharacter(string className)
    {
        List<string> cards = new List<string>();
        
        switch (className)
        {
            case "전사":
                cards.Add("타격");
                cards.Add("방어");
                cards.Add("강타");
                cards.Add("철벽");
                cards.Add("분노");
                cards.Add("광전사");
                break;
                
            case "마법사":
                cards.Add("화염구");
                cards.Add("방어막");
                cards.Add("번개");
                cards.Add("얼음 창");
                cards.Add("마나 실드");
                cards.Add("집중");
                break;
                
            case "도적":
                cards.Add("암습");
                cards.Add("회피");
                cards.Add("독칼");
                cards.Add("연막탄");
                cards.Add("그림자 숨기");
                cards.Add("급소 찌르기");
                break;
        }
        
        return cards;
    }
    
    // CardData 생성 (RewardManager와 동일)
    CardData CreateCardData(string cardName, int characterIndex)
    {
        CardData card;
        
        switch (cardName)
        {
            // 전사
            case "타격":
                return new CardData("타격", CardType.Attack, 1, 6, "적에게 6의 피해를 준다.", characterIndex);
            case "방어":
                return new CardData("방어", CardType.Skill, 1, 5, "5의 방어도를 얻는다.", characterIndex);
            case "강타":
                return new CardData("강타", CardType.Attack, 2, 12, "적에게 12의 피해를 준다.", characterIndex);
            case "철벽":
                return new CardData("철벽", CardType.Skill, 1, 8, "8의 방어도를 얻는다.", characterIndex);
            case "분노":
                return new CardData("분노", CardType.Power, 0, 3, "다음 공격 +3 (미구현)", characterIndex);
            case "광전사":
                card = new CardData("광전사", CardType.Attack, 1, 8, "적에게 8의 피해. 자신도 2 피해.", characterIndex);
                card.specialEffect = CardEffect.SelfDamage;
                card.effectValue = 2;
                return card;
                
            // 마법사
            case "화염구":
                return new CardData("화염구", CardType.Attack, 2, 10, "적에게 10의 피해를 준다.", characterIndex);
            case "방어막":
                return new CardData("방어막", CardType.Skill, 1, 8, "8의 방어도를 얻는다.", characterIndex);
            case "번개":
                return new CardData("번개", CardType.Attack, 1, 7, "적에게 7의 피해를 준다.", characterIndex);
            case "얼음 창":
                return new CardData("얼음 창", CardType.Attack, 2, 10, "적에게 10의 피해. 약화 (미구현)", characterIndex);
            case "마나 실드":
                card = new CardData("마나 실드", CardType.Skill, 1, 5, "5의 방어도. 카드 1장 드로우.", characterIndex);
                card.specialEffect = CardEffect.DrawCard;
                card.effectValue = 1;
                return card;
            case "집중":
                card = new CardData("집중", CardType.Skill, 0, 0, "에너지 +1", characterIndex);
                card.specialEffect = CardEffect.GainEnergy;
                card.effectValue = 1;
                return card;
                
            // 도적
            case "암습":
                return new CardData("암습", CardType.Attack, 1, 8, "적에게 8의 피해를 준다.", characterIndex);
            case "회피":
                return new CardData("회피", CardType.Skill, 1, 6, "6의 방어도를 얻는다.", characterIndex);
            case "독칼":
                return new CardData("독칼", CardType.Attack, 1, 4, "적에게 4의 피해. 독 3턴 (미구현)", characterIndex);
            case "연막탄":
                return new CardData("연막탄", CardType.Skill, 1, 10, "10의 방어도를 얻는다.", characterIndex);
            case "그림자 숨기":
                card = new CardData("그림자 숨기", CardType.Skill, 1, 6, "6의 방어도. 카드 1장 드로우.", characterIndex);
                card.specialEffect = CardEffect.DrawCard;
                card.effectValue = 1;
                return card;
            case "급소 찌르기":
                return new CardData("급소 찌르기", CardType.Attack, 2, 14, "적에게 14의 피해를 준다.", characterIndex);
                
            default:
                Debug.LogWarning($"알 수 없는 카드: {cardName}");
                return null;
        }
    }
    
    // 카드 구매
    void BuyCard(CardData card, Button button)
    {
        if (GameData.Instance == null) return;
        
        if (GameData.Instance.gold >= cardPrice)
        {
            // 골드 차감
            GameData.Instance.gold -= cardPrice;
            
            // 카드 추가
            if (card.characterIndex < GameData.Instance.raidParty.Count)
            {
                GameData.Instance.raidParty[card.characterIndex].cardList.Add(card.cardName);
                Debug.Log($"{card.cardName} 구매! (남은 골드: {GameData.Instance.gold})");
            }
            
            // 버튼 비활성화
            button.interactable = false;
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "구매 완료";
            
            UpdateGoldUI();
        }
    }
    
    // 카드 제거 구매
    void BuyRemoveCard(Button button)
    {
        if (GameData.Instance == null) return;
        
        if (GameData.Instance.gold >= removeCardPrice)
        {
            // 골드 차감
            GameData.Instance.gold -= removeCardPrice;
            
            Debug.Log($"카드 제거 구매! (남은 골드: {GameData.Instance.gold})");
            
            // TODO: 카드 선택 팝업 띄우기
            // 지금은 임시로 랜덤 카드 제거
            RemoveRandomCard();
            
            // 버튼 비활성화
            button.interactable = false;
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "구매 완료";
            
            UpdateGoldUI();
        }
    }
    
    // 체력 회복 구매
    void BuyHeal(Button button)
    {
        if (GameData.Instance == null) return;
        
        if (GameData.Instance.gold >= healPrice)
        {
            // 골드 차감
            GameData.Instance.gold -= healPrice;
            
            // 파티 체력 회복 (25%)
            foreach (var character in GameData.Instance.raidParty)
            {
                int healAmount = Mathf.RoundToInt(character.maxHealth * 0.25f);
                character.currentHealth = Mathf.Min(character.currentHealth + healAmount, character.maxHealth);
            }
            
            Debug.Log($"체력 회복! (남은 골드: {GameData.Instance.gold})");
            
            // 버튼 비활성화
            button.interactable = false;
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "구매 완료";
            
            UpdateGoldUI();
        }
    }
    
    // 임시: 랜덤 카드 제거
    void RemoveRandomCard()
    {
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0) return;
        
        CharacterData character = GameData.Instance.raidParty[Random.Range(0, GameData.Instance.raidParty.Count)];
        
        if (character.cardList.Count > 0)
        {
            string removedCard = character.cardList[Random.Range(0, character.cardList.Count)];
            character.cardList.Remove(removedCard);
            Debug.Log($"{removedCard} 제거됨!");
        }
    }
    
    // 골드 UI 업데이트
    void UpdateGoldUI()
    {
        if (goldText != null && GameData.Instance != null)
        {
            goldText.text = $"골드: {GameData.Instance.gold}";
        }
    }
    
    // 상점 나가기
    void LeaveShop()
    {
        Debug.Log("상점 나가기!");
        
        // 다음 층으로 진행
        if (RunData.Instance != null)
        {
            RunData.Instance.AdvanceFloor();
        }
        
        SceneManager.LoadScene("DungeonMapScene");
    }
}