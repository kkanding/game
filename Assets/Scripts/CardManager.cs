using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("Card Prefab")]
    public CardDisplay cardDisplayPrefab;
    
    [Header("Hand")]
    public Transform handTransform;
    public int maxHandSize = 10;
    public int drawPerTurn = 5;
    
    [Header("Deck System")]
    private List<CardData> deck = new List<CardData>(); // 덱
    private List<CardData> hand = new List<CardData>(); // 손패 데이터
    private List<CardData> discardPile = new List<CardData>(); // 버리기 더미
    
    [Header("UI Cards")]
    private List<CardDisplay> handUI = new List<CardDisplay>(); // 손패 UI
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
		Debug.Log("CardManager Start() 호출!");
        InitializeDeck();
        ShuffleDeck();
        //DrawCards(drawPerTurn);
    }
    
    // 덱 초기화 (전투 시작 시)
    void InitializeDeck()
	{
		deck.Clear();
		hand.Clear();
		discardPile.Clear();
		
		if (GameData.Instance == null || GameData.Instance.raidParty.Count == 0)
		{
			Debug.LogError("GameData가 없습니다!");
			return;
		}
		
		Debug.Log("===== 덱 초기화 시작 =====");
		
		// GameData에서 최신 덱 로드!
		for (int i = 0; i < GameData.Instance.raidParty.Count; i++)
		{
			CharacterData charData = GameData.Instance.raidParty[i];
			
			Debug.Log($"캐릭터 {charData.characterName} 덱 로드:");
			Debug.Log($"  카드 목록: {string.Join(", ", charData.cardList)}");
			
			// 쓰러진 캐릭이면(정신력 0 포함) 카드 전부 정신집중으로 유지
			bool isDown = charData.isDefeated || charData.currentMentalPower <= 0;

			foreach (string cardName in charData.cardList)
			{
				CardData cardData = isDown ? CreateReviveCard(i) : CreateCardData(cardName, i);
				if (cardData != null)
				{
					deck.Add(cardData);

					// 디버그 로그(원하면 지워)
					Debug.Log(isDown
						? $"  - 정신집중 추가(Down)"
						: $"  - {cardName} 추가");
				}
			}
		}
		
		Debug.Log($"덱 초기화 완료! 총 {deck.Count}장");
		Debug.Log("===== 덱 초기화 끝 =====");
	}
    
    // 카드 데이터 생성
    CardData CreateCardData(string cardName, int characterIndex)
    {
        CardData card;
        
        // 업그레이드 확인
        bool isUpgraded = cardName.EndsWith("+");
        string baseName = cardName.Replace("+", "");
        
        switch (baseName)
        {
            // ===== 전사 카드 (ATK 100, DEF 50) =====
            case "타격":
                card = new CardData("타격", CardType.Attack, 1, 90, "공격력의 90% 피해", characterIndex);
                // ← 배율 설정은 Card 컴포넌트에서 하므로 여기선 계산된 값만 표시
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "방어":
                card = new CardData("방어", CardType.Skill, 1, 65, "방어력의 130% 방어도", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "강타":
                card = new CardData("강타", CardType.Attack, 2, 180, "공격력의 180% 피해", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
                
            case "철벽":
                card = new CardData("철벽", CardType.Skill, 2, 130, "방어력의 260% 방어도", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "분노":
                card = new CardData("분노", CardType.Power, 0, 3, "다음 공격 +3 (미구현)", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "광전사":
                card = new CardData("광전사", CardType.Attack, 1, 120, "공격력의 120% 피해. 자신도 2 피해", characterIndex);
                card.specialEffect = CardEffect.SelfDamage;
                card.effectValue = 2;
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            // ===== 마법사 카드 (ATK 130, DEF 30) =====
            case "화염구":
                card = new CardData("화염구", CardType.Attack, 1, 110, "공격력의 85% 피해", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "방어막":
            case "마법 방벽":
                card = new CardData("마법 방벽", CardType.Skill, 1, 45, "방어력의 150% 방어도", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "번개":
                card = new CardData("번개", CardType.Attack, 1, 104, "공격력의 80% 피해", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "얼음 창":
                card = new CardData("얼음 창", CardType.Attack, 2, 195, "공격력의 150% 피해. 약화 (미구현)", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "마나 실드":
                card = new CardData("마나 실드", CardType.Skill, 1, 42, "방어력의 140% 방어도. 카드 1장 드로우", characterIndex);
                card.specialEffect = CardEffect.DrawCard;
                card.effectValue = 1;
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "집중":
            case "명상":
                card = new CardData("명상", CardType.Skill, 1, 25, "정신력 25 회복", characterIndex);
                card.specialEffect = CardEffect.GainEnergy; // ← 나중에 정신력 회복 전용 이펙트로 변경
                card.effectValue = 25;
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            // ===== 도적 카드 (ATK 120, DEF 30) =====
            case "암습":
                card = new CardData("암습", CardType.Attack, 1, 108, "공격력의 90% 피해", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "회피":
                card = new CardData("회피", CardType.Skill, 1, 42, "방어력의 140% 방어도", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "독칼":
                card = new CardData("독칼", CardType.Attack, 1, 72, "공격력의 60% 피해. 독 3턴 (미구현)", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "연막탄":
                card = new CardData("연막탄", CardType.Skill, 1, 54, "방어력의 180% 방어도", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "그림자 숨기":
                card = new CardData("그림자 숨기", CardType.Skill, 1, 42, "방어력의 140% 방어도. 카드 1장 드로우", characterIndex);
                card.specialEffect = CardEffect.DrawCard;
                card.effectValue = 1;
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            case "급소 찌르기":
                card = new CardData("급소 찌르기", CardType.Attack, 2, 216, "공격력의 180% 피해", characterIndex);
                if (isUpgraded) card = card.Upgrade();
                return card;
            
            // ===== 정신집중 카드 (부활용) =====
            case "정신집중":
                card = new CardData("정신집중", CardType.Skill, 0, 0, "정신을 집중한다...", characterIndex);
                card.specialEffect = CardEffect.None;
                return card;
            
            default:
                Debug.LogWarning($"알 수 없는 카드: {cardName}");
                return null;
        }
    }
    
    // 덱 셔플
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        
        Debug.Log("덱 셔플 완료!");
    }
    
    // 카드 드로우
    public void DrawCards(int amount)
	{
		Debug.Log($"카드 {amount}장 드로우 시도...");
		
		int drawnCount = 0;
		List<CardDisplay> newCards = new List<CardDisplay>();
		
		for (int i = 0; i < amount; i++)
		{
			if (hand.Count >= maxHandSize)
			{
				Debug.Log($"손패가 가득 찼습니다! (최대 {maxHandSize}장)");
				break;
			}
			
			if (deck.Count == 0)
			{
				if (discardPile.Count == 0)
				{
					Debug.Log("덱과 버리기 더미가 모두 비었습니다!");
					break;
				}
				
				Debug.Log($"덱이 비었습니다! 버리기 더미({discardPile.Count}장)를 덱으로 이동 후 셔플...");
				deck.AddRange(discardPile);
				discardPile.Clear();
				ShuffleDeck();
			}
			
			CardData drawnCard = deck[0];
			deck.RemoveAt(0);
			hand.Add(drawnCard);
			
			// UI 생성 (애니메이션 없이)
			CardDisplay cardDisplay = CreateCardUIWithoutAnimation(drawnCard);
			if (cardDisplay != null)
			{
				newCards.Add(cardDisplay);
			}
			
			drawnCount++;
		}
		
		Debug.Log($"드로우 완료! {drawnCount}장 뽑음 / 손패: {hand.Count}장, 덱: {deck.Count}장, 버리기: {discardPile.Count}장");
		
		// 레이아웃 적용 후 애니메이션 실행
		if (newCards.Count > 0)
		{
			StartCoroutine(PlayDrawAnimationsAfterLayout(newCards));
		}
		
		BattleUI.Instance?.UpdateDeckUI();
	}
    
    // 카드 사용
    public void PlayCard(CardDisplay cardDisplay)
	{
		Card card = cardDisplay.GetComponent<Card>();
		if (card == null) return;
		
		// 손패 데이터에서 제거
		CardData cardData = hand.Find(c => 
			c.cardName == card.cardName && 
			c.characterIndex == card.characterIndex);
		
		if (cardData != null)
		{
			hand.Remove(cardData);
			discardPile.Add(cardData);
		}
		
		// UI에서 제거
		handUI.Remove(cardDisplay);
		
		Debug.Log($"{card.cardName} 버리기 더미로! (버리기: {discardPile.Count}장)");
		
		// 버리기 애니메이션 재생
		CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
		if (animator != null)
		{
			animator.PlayDiscardAnimation(() => 
			{
				Destroy(cardDisplay.gameObject);
				// 삭제 후 프레임 대기하고 재정렬
				StartCoroutine(DelayedReorganize());
			});
		}
		else
		{
			Destroy(cardDisplay.gameObject);
			StartCoroutine(DelayedReorganize());
		}
		
		BattleUI.Instance?.UpdateDeckUI();
	}
    
    // 턴 종료 시 - 모든 손패 버리기
    public void EndTurn()
    {
        Debug.Log("턴 종료! 모든 손패를 버립니다...");
        
        // 모든 손패를 버리기 더미로
        discardPile.AddRange(hand);
        hand.Clear();
        
        // UI 전부 제거
        foreach (var cardUI in handUI)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }
        handUI.Clear();
        
        Debug.Log($"손패 버림 완료! (버리기: {discardPile.Count}장)");
		
		// 마지막 줄에 추가
		BattleUI.Instance?.UpdateDeckUI();
    }
    
    // 턴 시작 시 - 카드 드로우
    public void StartTurn()
    {
        Debug.Log("CardManager StartTurn() 호출!");
        DrawCards(drawPerTurn);
		
		// ← 드로우 후 재정렬 추가!
		StartCoroutine(DelayedReorganizeAfterTurnStart());
    }
	
	// 덱 장수 반환
	public int GetDeckCount()
	{
		return deck.Count;
	}

	// 버리기 더미 장수 반환
	public int GetDiscardCount()
	{
		return discardPile.Count;
	}
	
	// 손패 장수 반환
	public int GetHandCount()
	{
		return hand.Count;
	}

	// 덱 내용 보기
	public void ShowDeckView()
	{
		Debug.Log("=== 덱 내용 ===");
		if (deck.Count == 0)
		{
			Debug.Log("덱이 비어있습니다!");
			return;
		}
		
		foreach (var card in deck)
		{
			Debug.Log($"- {card.cardName} (코스트:{card.cost})");
		}
		Debug.Log($"총 {deck.Count}장");
	}

	// 버리기 더미 내용 보기
	public void ShowDiscardView()
	{
		Debug.Log("=== 버리기 더미 내용 ===");
		if (discardPile.Count == 0)
		{
			Debug.Log("버리기 더미가 비어있습니다!");
			return;
		}
		
		foreach (var card in discardPile)
		{
			Debug.Log($"- {card.cardName} (코스트:{card.cost})");
		}
		Debug.Log($"총 {discardPile.Count}장");
	}
	
	// 손패 재정렬 (빈 공간 제거)
	void ReorganizeHand()
	{
		if (handTransform == null) return;
		
		// 먼저 모든 null 참조 제거
		handUI.RemoveAll(card => card == null);
		
		// Layout 강제 업데이트 (여러 번!)
		Canvas.ForceUpdateCanvases();
		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(handTransform as RectTransform);
		
		// 모든 카드의 layoutPosition 리셋
		foreach (var card in handUI)
		{
			if (card != null)
			{
				CardAnimator animator = card.GetComponent<CardAnimator>();
				if (animator != null)
				{
					animator.ResetLayoutPosition();
				}
			}
		}
		
		Debug.Log($"손패 재정렬! 현재 {handUI.Count}장");
	}

	System.Collections.IEnumerator ResetCardPositions()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		// 모든 카드의 애니메이터에게 위치 리셋 알림
		foreach (var card in handUI)
		{
			if (card != null)
			{
				CardAnimator animator = card.GetComponent<CardAnimator>();
				if (animator != null)
				{
					animator.ResetLayoutPosition(); // ← 새 함수
				}
			}
		}
	}
	
	// 애니메이션 없이 카드 UI만 생성
	CardDisplay CreateCardUIWithoutAnimation(CardData cardData)
	{
		if (cardDisplayPrefab == null || handTransform == null)
		{
			Debug.LogError("Prefab 또는 Hand가 없습니다!");
			return null;
		}
		
		CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, handTransform);
		
		Card card = cardDisplay.gameObject.AddComponent<Card>();
		card.SetFromCardData(cardData);
		cardDisplay.SetupCard(card);
		
		handUI.Add(cardDisplay);
		
		return cardDisplay;
	}

	// 레이아웃 적용 후 드로우 애니메이션 실행
	System.Collections.IEnumerator PlayDrawAnimationsAfterLayout(List<CardDisplay> cards)
	{
		// 레이아웃이 완전히 적용될 때까지 충분히 대기
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame(); // ← 한 번 더!
		
		// Layout 강제 업데이트
		if (handTransform != null)
		{
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(handTransform as RectTransform);
		}
		
		yield return new WaitForEndOfFrame(); // ← 한 번 더!
		
		// 각 카드의 layoutPosition 업데이트
		foreach (var cardDisplay in cards)
		{
			if (cardDisplay != null)
			{
				CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
				if (animator != null)
				{
					animator.ResetLayoutPosition(); // ← 위치 기록
				}
			}
		}
		
		// 이제 애니메이션 실행
		foreach (var cardDisplay in cards)
		{
			if (cardDisplay != null)
			{
				CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
				if (animator != null)
				{
					animator.PlayDrawAnimation();
				}
			}
		}
	}
	
	// 지연된 재정렬
	System.Collections.IEnumerator DelayedReorganize()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		ReorganizeHand();
	}
	
	// 턴 시작 드로우 후 재정렬
	System.Collections.IEnumerator DelayedReorganizeAfterTurnStart()
	{
		// 드로우 애니메이션이 시작되기 전에 충분히 대기
		yield return new WaitForSeconds(0.1f);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		ReorganizeHand();
	}
	
	// 덱 리스트 반환
	public List<CardData> GetDeckList()
	{
		return new List<CardData>(deck); // 복사본 반환
	}

	// 버리기 더미 리스트 반환
	public List<CardData> GetDiscardList()
	{
		return new List<CardData>(discardPile); // 복사본 반환
	}
	
	// 카드 업그레이드
	public void UpgradeCard(string cardName, int characterIndex)
	{
		if (GameData.Instance == null) return;
		
		CharacterData character = GameData.Instance.raidParty[characterIndex];
		
		// 카드 찾기
		int cardIndex = character.cardList.FindIndex(c => c == cardName);
		
		if (cardIndex >= 0)
		{
			// 이미 업그레이드된 카드인지 확인
			if (character.cardList[cardIndex].EndsWith("+"))
			{
				Debug.Log($"{cardName}는 이미 업그레이드되었습니다!");
				return;
			}
			
			// 업그레이드
			character.cardList[cardIndex] = cardName + "+";
			Debug.Log($"{cardName} → {cardName}+ 업그레이드!");
		}
	}

	// 카드 제거
	public void RemoveCard(string cardName, int characterIndex)
	{
		if (GameData.Instance == null) return;
		
		CharacterData character = GameData.Instance.raidParty[characterIndex];
		
		// 카드 찾아서 제거
		character.cardList.Remove(cardName);
		Debug.Log($"{cardName} 제거!");
	}
	
	// ======= 정신집중 & 부활 시스템 =======
    
    // 캐릭터의 카드를 정신집중으로 변경
    public void ConvertCharacterCardsToRevive(CharacterData character)
    {
        if (character == null || GameData.Instance == null) return;
        
        // ← characterIndex 찾기
        int charIndex = GameData.Instance.raidParty.IndexOf(character);
        if (charIndex < 0) return;
        
        Debug.Log($"===== {character.characterName}의 카드를 정신집중으로 변경! (Index: {charIndex}) =====");
        
        // 덱에서 변경
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].characterIndex == charIndex)
            {
                deck[i] = CreateReviveCard(charIndex);
            }
        }
        
        // 손패에서 변경
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i].characterIndex == charIndex)
            {
                hand[i] = CreateReviveCard(charIndex);
            }
        }
        
        // 버리기 더미에서 변경
        for (int i = 0; i < discardPile.Count; i++)
        {
            if (discardPile[i].characterIndex == charIndex)
            {
                discardPile[i] = CreateReviveCard(charIndex);
            }
        }
        
        // 손패 UI 업데이트
        UpdateHandUI();
        
        Debug.Log($"변환 완료! 덱: {deck.Count}, 손패: {hand.Count}, 버리기: {discardPile.Count}");
    }
    
    // 정신집중 카드 생성
    CardData CreateReviveCard(int characterIndex)
    {
        CardData reviveCard = new CardData("정신집중", CardType.Skill, 0, 0, "정신을 집중한다...", characterIndex);
        reviveCard.specialEffect = CardEffect.None;
        return reviveCard;
    }
    
    // 캐릭터 카드 복구 (부활 시)
    public void RestoreCharacterCards(CharacterData character)
    {
        if (character == null || GameData.Instance == null) return;
        
        // ← characterIndex 찾기
        int charIndex = GameData.Instance.raidParty.IndexOf(character);
        if (charIndex < 0) return;
        
        Debug.Log($"===== {character.characterName}의 카드 복구! (Index: {charIndex}) =====");
        
        List<string> originalCards = character.cardList;
        
        // 덱에서 복구
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].characterIndex == charIndex && deck[i].cardName == "정신집중")
            {
                string cardName = originalCards[Random.Range(0, originalCards.Count)];
                deck[i] = CreateCardData(cardName, charIndex);
            }
        }
        
        // 손패에서 복구
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i].characterIndex == charIndex && hand[i].cardName == "정신집중")
            {
                string cardName = originalCards[Random.Range(0, originalCards.Count)];
                hand[i] = CreateCardData(cardName, charIndex);
            }
        }
        
        // 버리기 더미에서 복구
        for (int i = 0; i < discardPile.Count; i++)
        {
            if (discardPile[i].characterIndex == charIndex && discardPile[i].cardName == "정신집중")
            {
                string cardName = originalCards[Random.Range(0, originalCards.Count)];
                discardPile[i] = CreateCardData(cardName, charIndex);
            }
        }
        
        // 손패 UI 업데이트
        UpdateHandUI();
        
        Debug.Log("카드 복구 완료!");
    }
    
    // 손패 UI 업데이트
    void UpdateHandUI()
    {
        // 기존 손패 UI 전부 제거
        foreach (var cardUI in handUI)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }
        handUI.Clear();
        
        // 새로 생성
        foreach (var cardData in hand)
        {
            CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, handTransform);
            Card cardComponent = cardDisplay.gameObject.AddComponent<Card>();
            cardComponent.SetFromCardData(cardData);
            cardDisplay.SetupCard(cardComponent);
            handUI.Add(cardDisplay);
        }
        
        // 레이아웃 재정렬
        StartCoroutine(DelayedReorganize());
    }
}