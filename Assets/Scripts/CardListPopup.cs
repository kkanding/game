using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class CardListPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public Transform cardContainer;
    public CardDisplay cardDisplayPrefab;
    public Button closeButton;
	
	private System.Action<CardData> onCardClickCallback;
    
    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }
    
    // 덱 카드 목록 표시
    public void ShowDeck(List<CardData> deckCards)
    {
        if (titleText != null)
        {
            titleText.text = $"덱 목록 ({deckCards.Count}장)";
        }
        
        DisplayCards(deckCards);
    }
    
    // 버리기 더미 카드 목록 표시
    public void ShowDiscard(List<CardData> discardCards)
    {
        if (titleText != null)
        {
            titleText.text = $"버리기 더미 ({discardCards.Count}장)";
        }
        
        DisplayCards(discardCards);
    }
    
    // 카드 표시
    void DisplayCards(List<CardData> cards)
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
            
            // Card 컴포넌트 추가
            Card card = cardDisplay.gameObject.AddComponent<Card>();
            card.SetFromCardData(cardData);
            cardDisplay.SetupCard(card);
            
            // 애니메이터 비활성화 (팝업에서는 애니메이션 필요 없음)
            CardAnimator animator = cardDisplay.GetComponent<CardAnimator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            // 클릭 불가능하게
            Button btn = cardDisplay.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = false;
            }
        }
    }
    
    // 팝업 닫기
    public void Close()
    {
        // 콜백 초기화
		onCardClickCallback = null;
		
		// 파괴하지 말고 비활성화!
		gameObject.SetActive(false);
    }
	
	// ← 콜백과 함께 표시
    public void ShowWithCallback(List<CardData> cards, string title, System.Action<CardData> callback)
	{
		onCardClickCallback = callback;
		
		// ← 타이틀이 비어있으면 TitleText 숨기기!
		if (titleText != null)
		{
			if (string.IsNullOrEmpty(title))
			{
				titleText.gameObject.SetActive(false); // ← 숨기기!
			}
			else
			{
				titleText.gameObject.SetActive(true); // ← 보이기
				titleText.text = $"{title} ({cards.Count}장)";
			}
		}
		
		DisplayCardsWithCallback(cards);
		
		// 명시적으로 활성화!
		gameObject.SetActive(true);
	}
    
    // ← 콜백 버튼과 함께 카드 표시
    void DisplayCardsWithCallback(List<CardData> cards)
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
			
			// ← 부모 없이 생성 후 SetParent!
			CardDisplay cardDisplay = Instantiate(cardDisplayPrefab);
			cardDisplay.transform.SetParent(cardContainer, false);
			
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
			
			// Button 추가
			Button btn = cardDisplay.GetComponent<Button>();
			if (btn == null)
			{
				btn = cardDisplay.gameObject.AddComponent<Button>();
			}
			
			btn.interactable = true;
			btn.onClick.RemoveAllListeners();
			
			// 로컬 변수
			CardData localCard = cardData;
			
			btn.onClick.AddListener(() => {
				Debug.Log($"카드 선택: {localCard.cardName}");
				if (onCardClickCallback != null)
				{
					onCardClickCallback(localCard);
					Close(); // 선택 후 팝업 닫기
				}
			});
		}
	}
}