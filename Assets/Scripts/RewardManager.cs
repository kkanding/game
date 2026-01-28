using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RewardManager : MonoBehaviour
{
	[Header("UI References")]
	public TextMeshProUGUI titleText;
	public TextMeshProUGUI descriptionText; // ← 추가!
	public Transform cardContainer;
	public CardDisplay cardDisplayPrefab;
	public Button closeButton;

	[Header("Reward Type Buttons")] // ← 추가!
	public Button addCardButton;
	public Button upgradeCardButton;
	public Button removeCardButton;
	public GameObject rewardTypePanel;
	
	[Header("Popup")] // ← 추가!
	public CardListPopup cardListPopup; // CardListPopup Prefab 또는 인스턴스

	private enum RewardType
	{
		AddCard,
		UpgradeCard,
		RemoveCard
	}

	private RewardType currentRewardType = RewardType.AddCard;
	
    public static RewardManager Instance;
    
    private List<CardData> rewardCards = new List<CardData>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
	{
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Close);
		}
		
		// ← 버튼 이벤트 추가!
		if (addCardButton != null)
		{
			addCardButton.onClick.AddListener(() => ShowRewardType(RewardType.AddCard));
		}
		
		if (upgradeCardButton != null)
		{
			upgradeCardButton.onClick.AddListener(() => ShowRewardType(RewardType.UpgradeCard));
		}
		
		if (removeCardButton != null)
		{
			removeCardButton.onClick.AddListener(() => ShowRewardType(RewardType.RemoveCard));
		}
		
		// 기본: 카드 추가
		ShowRewardType(RewardType.AddCard);
	}
    
    // 보상 카드 3장 생성
    void GenerateRewardCards()
    {
        rewardCards.Clear();
        
        if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0)
        {
            Debug.LogError("GameData가 없습니다!");
            return;
        }
        
        // 각 캐릭터별로 1장씩 무작위 선택 (총 3장)
        for (int i = 0; i < 3 && i < GameData.Instance.raidParty.Count; i++)
        {
            CharacterData character = GameData.Instance.raidParty[i];
            
            // 해당 캐릭터의 모든 카드 중 랜덤 1장
			List<string> availableCards = GetAllCardsForCharacter(character.characterName);
            
            if (availableCards.Count > 0)
            {
                string randomCardName = availableCards[Random.Range(0, availableCards.Count)];
                CardData cardData = CreateCardData(randomCardName, i);
                
                if (cardData != null)
                {
                    rewardCards.Add(cardData);
                }
            }
        }
        
        Debug.Log($"보상 카드 {rewardCards.Count}장 생성 완료!");
    }
    
    // 캐릭터별 사용 가능한 모든 카드 목록
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
    
    // 카드 데이터 생성 (CardManager와 동일)
    CardData CreateCardData(string cardName, int characterIndex)
    {
        // CardManager의 CreateCardData()와 동일한 로직
        // 간단하게 CardManager의 메서드를 public으로 만들어서 재사용하는 것도 좋습니다!
        
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
    
    // 보상 카드 UI 표시
    void DisplayRewardCards()
	{
		// 기존 임시 카드 제거
		foreach (Transform child in cardContainer)
		{
			Destroy(child.gameObject);
		}
		
		// 새 카드 생성
		foreach (CardData cardData in rewardCards)
		{
			CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, cardContainer);
			
			Card card = cardDisplay.gameObject.AddComponent<Card>();
			card.SetFromCardData(cardData);
			cardDisplay.SetupCard(card);
			
			// ← CardAnimator 비활성화!
			CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
			if (animator != null)
			{
				animator.enabled = false; // 애니메이션 비활성화!
			}
			
			// 크기 강제 설정
			RectTransform cardRect = cardDisplay.GetComponent<RectTransform>();
			if (cardRect != null)
			{
				cardRect.sizeDelta = new Vector2(200, 280);
			}
			
			// Layout Element 확인
			LayoutElement layoutElement = cardDisplay.GetComponent<LayoutElement>();
			if (layoutElement == null)
			{
				layoutElement = cardDisplay.gameObject.AddComponent<LayoutElement>();
			}
			layoutElement.preferredWidth = 200;
			layoutElement.preferredHeight = 280;
			
			// 클릭 이벤트 추가
			cardDisplay.gameObject.AddComponent<RewardCardSelector>();
		}
		
		// Layout 강제 업데이트
		Canvas.ForceUpdateCanvases();
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);
	}

	// Layout 재구성
	System.Collections.IEnumerator RebuildLayout(HorizontalLayoutGroup layout)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		// Layout 다시 활성화
		if (layout != null)
		{
			layout.enabled = true;
		}
		
		// 강제 업데이트
		Canvas.ForceUpdateCanvases();
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);
		
		yield return new WaitForEndOfFrame();
		
		// 한 번 더!
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);
	}
    
    // 카드 선택 처리
    public void SelectCard(CardData cardData)
    {
        Debug.Log($"{cardData.cardName} 선택!");
        
        // GameData의 해당 캐릭터 덱에 추가
        if (GameData.Instance != null && cardData.characterIndex < GameData.Instance.raidParty.Count)
        {
            GameData.Instance.raidParty[cardData.characterIndex].cardList.Add(cardData.cardName);
            Debug.Log($"{cardData.cardName} 덱에 추가 완료!");
        }
        
        // 로비로 복귀
        ReturnToLobby();
    }
    
    // 스킵
    public void SkipReward()
    {
        Debug.Log("보상 스킵!");
        ReturnToLobby();
    }
    
    // 팝업 닫기 (로비로)
	public void Close()
	{
		ReturnToLobby();
	}

	void ReturnToLobby()
	{
		Debug.Log("===== ReturnToLobby 호출 =====");
		Debug.Log($"RunData.Instance: {RunData.Instance}");
		
		if (RunData.Instance != null)
		{
			Debug.Log($"isInDungeon: {RunData.Instance.isInDungeon}");
			Debug.Log($"currentFloor: {RunData.Instance.currentFloor}");
		}
		
		// 던전 진행 중이면 맵으로!
		if (RunData.Instance != null && RunData.Instance.isInDungeon)
		{
			Debug.Log("맵으로 복귀!");
			// 전투 보상 화면이므로, 여기서 다음 층으로 진행
			RunData.Instance.AdvanceFloor();
			SceneManager.LoadScene("DungeonMapScene");
		}
		else
		{
			Debug.Log("로비로 복귀!");
			SceneManager.LoadScene("LobbyScene");
		}
	}
	
	// 보상 타입 선택
	void ShowRewardType(RewardType type)
	{
		currentRewardType = type;
		
		// ← 모든 타입 전환 시 팝업 닫기!
		if (cardListPopup != null && cardListPopup.gameObject.activeSelf)
		{
			cardListPopup.Close();
		}
		
		switch (type)
		{
			case RewardType.AddCard:
				// 카드 컨테이너 다시 활성화!
				if (cardContainer != null)
				{
					cardContainer.gameObject.SetActive(true);
				}
				
				GenerateRewardCards();
				DisplayRewardCards();
				
				if (titleText != null)
				{
					titleText.text = "보상 선택";
				}
				if (descriptionText != null)
				{
					descriptionText.text = "카드를 선택하세요";
				}
				break;
				
			case RewardType.UpgradeCard:
				ShowUpgradeOptions();
				break;
				
			case RewardType.RemoveCard:
				ShowRemoveOptions();
				break;
		}
	}

	// 업그레이드 옵션 표시
	void ShowUpgradeOptions()
	{
		// 카드 컨테이너 숨기기
		if (cardContainer != null)
		{
			cardContainer.gameObject.SetActive(false);
		}
		
		// 전체 덱 가져오기
		if (GameData.Instance != null)
		{
			List<CardData> allCards = new List<CardData>();
			
			for (int i = 0; i < GameData.Instance.raidParty.Count; i++)
			{
				CharacterData character = GameData.Instance.raidParty[i];
				
				foreach (string cardName in character.cardList)
				{
					// 이미 업그레이드된 카드는 제외
					if (!cardName.EndsWith("+"))
					{
						CardData cardData = CreateCardData(cardName, i);
						if (cardData != null)
						{
							allCards.Add(cardData);
						}
					}
				}
			}
			
			// ← TitleText와 DescriptionText 설정
			if (titleText != null)
			{
				titleText.text = "카드 강화";
			}
			if (descriptionText != null)
			{
				descriptionText.text = $"강화할 카드를 선택하세요 ({allCards.Count}장)";
			}
			
			// 팝업 표시 (빈 타이틀)
			if (cardListPopup != null)
			{
				cardListPopup.ShowWithCallback(allCards, "", UpgradeSelectedCard); // ← 빈 문자열!
			}
			else
			{
				Debug.LogError("CardListPopup이 없습니다!");
			}
		}
	}
	
	// 업그레이드 가능한 카드 표시
	void DisplayUpgradeCards(List<CardData> cards)
	{
		// 기존 카드 제거
		foreach (Transform child in cardContainer)
		{
			Destroy(child.gameObject);
		}
		
		// 새 카드 생성
		foreach (CardData cardData in cards)
		{
			if (cardDisplayPrefab == null) continue;
			
			CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, cardContainer);
			
			Card card = cardDisplay.gameObject.AddComponent<Card>();
			card.SetFromCardData(cardData);
			cardDisplay.SetupCard(card);
			
			// OnPointerClick 비활성화
			cardDisplay.disableClick = true;
			
			// 애니메이터 비활성화
			CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
			if (animator != null)
			{
				animator.enabled = false;
			}
			
			// ← Button 확인 및 추가
			Button btn = cardDisplay.GetComponent<Button>();
			if (btn == null)
			{
				btn = cardDisplay.gameObject.AddComponent<Button>();
			}
			
			// Button 설정
			btn.interactable = true;
			btn.onClick.RemoveAllListeners();
			
			// 로컬 변수로 복사
			CardData localCard = cardData;
			
			btn.onClick.AddListener(() => {
				Debug.Log($"업그레이드 버튼 클릭: {localCard.cardName}");
				UpgradeSelectedCard(localCard);
			});
		}
	}

	// 선택한 카드 업그레이드
	void UpgradeSelectedCard(CardData cardData)
	{
		Debug.Log($"{cardData.cardName} 업그레이드!");
		
		// GameData에서 카드 업그레이드
		if (GameData.Instance != null && cardData.characterIndex < GameData.Instance.raidParty.Count)
		{
			CharacterData character = GameData.Instance.raidParty[cardData.characterIndex];
			
			// 카드 이름 찾아서 + 추가
			int index = character.cardList.FindIndex(c => c == cardData.cardName);
			if (index >= 0)
			{
				character.cardList[index] = cardData.cardName + "+";
				Debug.Log($"{cardData.cardName} → {cardData.cardName}+ 업그레이드 완료!");
			}
		}
		
		// 로비로 복귀
		ReturnToLobby();
	}
	
	// 제거 가능한 카드 표시
	void DisplayRemoveCards(List<CardData> cards)
	{
		// 기존 카드 제거
		foreach (Transform child in cardContainer)
		{
			Destroy(child.gameObject);
		}
		
		// 새 카드 생성
		foreach (CardData cardData in cards)
		{
			if (cardDisplayPrefab == null) continue;
			
			CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, cardContainer);
			
			Card card = cardDisplay.gameObject.AddComponent<Card>();
			card.SetFromCardData(cardData);
			cardDisplay.SetupCard(card);
			
			// OnPointerClick 비활성화
			cardDisplay.disableClick = true;
			
			// 애니메이터 비활성화
			CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
			if (animator != null)
			{
				animator.enabled = false;
			}
			
			// ← Button 확인 및 추가
			Button btn = cardDisplay.GetComponent<Button>();
			if (btn == null)
			{
				btn = cardDisplay.gameObject.AddComponent<Button>();
			}
			
			// Button 설정
			btn.interactable = true;
			btn.onClick.RemoveAllListeners();
			
			// 로컬 변수로 복사
			CardData localCard = cardData;
			
			btn.onClick.AddListener(() => {
				Debug.Log($"제거 버튼 클릭: {localCard.cardName}");
				RemoveSelectedCard(localCard);
			});
		}
	}

	// 선택한 카드 제거
	void RemoveSelectedCard(CardData cardData)
	{
		Debug.Log($"{cardData.cardName} 제거!");
		
		// GameData에서 카드 제거
		if (GameData.Instance != null && cardData.characterIndex < GameData.Instance.raidParty.Count)
		{
			CharacterData character = GameData.Instance.raidParty[cardData.characterIndex];
			
			// 카드 제거
			character.cardList.Remove(cardData.cardName);
			Debug.Log($"{cardData.cardName} 제거 완료!");
		}
		
		// 로비로 복귀
		ReturnToLobby();
	}

	// 제거 옵션 표시
	void ShowRemoveOptions()
	{
		// 카드 컨테이너 숨기기
		if (cardContainer != null)
		{
			cardContainer.gameObject.SetActive(false);
		}
		
		// 전체 덱 가져오기
		if (GameData.Instance != null)
		{
			List<CardData> allCards = new List<CardData>();
			
			for (int i = 0; i < GameData.Instance.raidParty.Count; i++)
			{
				CharacterData character = GameData.Instance.raidParty[i];
				
				foreach (string cardName in character.cardList)
				{
					CardData cardData = CreateCardData(cardName, i);
					if (cardData != null)
					{
						allCards.Add(cardData);
					}
				}
			}
			
			// ← TitleText와 DescriptionText 설정
			if (titleText != null)
			{
				titleText.text = "카드 제거";
			}
			if (descriptionText != null)
			{
				descriptionText.text = $"제거할 카드를 선택하세요 ({allCards.Count}장)";
			}
			
			// 팝업 표시 (빈 타이틀)
			if (cardListPopup != null)
			{
				cardListPopup.ShowWithCallback(allCards, "", RemoveSelectedCard); // ← 빈 문자열!
			}
			else
			{
				Debug.LogError("CardListPopup이 없습니다!");
			}
		}
	}
}